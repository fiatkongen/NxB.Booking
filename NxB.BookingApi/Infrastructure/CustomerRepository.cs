using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Infrastructure
{
    public class CustomerRepository : TenantFilteredRepository<Customer, AppDbContext>, ICustomerRepository
    {
        public CustomerRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        protected override IQueryable<Customer> TenantFilteredEntitiesQuery
        {
            get
            {
                return base.TenantFilteredEntitiesQuery.Include(x => x.Accounts).Include(x => x.CustomerGroupCustomers);
            }
        }

        public void Add(Customer customer)
        {
            this.Serialize(customer);
            this.AppDbContext.Add(customer);
        }

        public void Add(IEnumerable<Customer> customers)
        {
            foreach (var customer in customers)
            {
                this.Add(customer);
            }
        }

        public void Delete(Guid id)
        {
            var customer = FindSingle(id);
            this.AppDbContext.Customers.Remove(customer);
        }

        public void MarkAsDeleted(Guid id)
        {
            var customer = FindSingle(id);
            if (customer != null)
                customer.IsDeleted = true;
        }

        public void Undelete(Guid id)
        {
            var customer = FindSingleIncludeDeleted(id);
            if (customer != null)
                customer.IsDeleted = false;
        }

        public void UpdateCustomerNote(Guid customerId, string note, bool? noteState)
        {
            var customer = base.TenantFilteredEntitiesQuery.Single(x => x.Id == customerId);
            if (noteState == null && note == customer.Note) return;

                customer.Note = string.IsNullOrWhiteSpace(note) ? null : note;
            if (noteState != null)
            {
                customer.NoteState = noteState.Value;
            }

            if (string.IsNullOrWhiteSpace(customer.Note))
            {
                customer.NoteState = false;
            }
            this.AppDbContext.Update(customer);
        }

        public int DeleteAllImported(Guid tenantId)
        {
            var importedEntitiesCount =
                this.AppDbContext.Database.ExecuteSqlRaw(
                    $"DELETE FROM CUSTOMER WHERE TenantId = '{tenantId}' AND IsImported=1");
            return importedEntitiesCount;
        }

        public Customer FindSingle(Guid id, bool includeDeleted = false)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var customer = this.TenantFilteredEntitiesQuery.Single(x => x.Id == id && (!x.IsDeleted || x.IsDeleted == includeDeleted));
            this.Deserialize(customer);
            return customer;
        }

        public Customer FindSingleOrDefault(Guid id, bool includeDeleted = false)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var customer = this.TenantFilteredEntitiesQuery.SingleOrDefault(x => x.Id == id && (!x.IsDeleted || x.IsDeleted == includeDeleted));
            this.Deserialize(customer);
            return customer;
        }

        public Customer FindSingleFromAccountId(Guid accountId, bool includeDeleted = false)
        {
            if (accountId == Guid.Empty) throw new ArgumentException(nameof(accountId));
            var customer = this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted || x.IsDeleted == includeDeleted).Join(AppDbContext.Accounts.Where(a => a.Id == accountId), c => c.Id, a => a.CustomerId, (c, a) => c).Single();
            this.Deserialize(customer);
            return customer;
        }

        public Customer FindSingleFromAccountId(Guid accountId, Guid tenantId)
        {
            if (accountId == Guid.Empty) throw new ArgumentException(nameof(accountId));
            if (tenantId == Guid.Empty) throw new ArgumentException(nameof(tenantId));
            var customer = this.AppDbContext.Customers.Where(x => !x.IsDeleted && x.TenantId == tenantId).Join(AppDbContext.Accounts.Where(a => a.Id == accountId && a.TenantId == tenantId), c => c.Id, a => a.CustomerId, (c, a) => c).Single();
            this.Deserialize(customer);
            return customer;
        }

        public Customer FindSingleOrDefaultFromFriendlyId(long friendlyId)
        {
            var customer = this.TenantFilteredEntitiesQuery.SingleOrDefault(x => x.FriendlyId == friendlyId);
            if (customer == null) return null;
            this.Deserialize(customer);
            return customer;
        }

        public Customer FindSingleIncludeDeleted(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var customer = this.TenantFilteredEntitiesQuery.Single(x => x.Id == id);
            this.Deserialize(customer);
            return customer;
        }

        public async Task<IList<Customer>> FindAll()
        {
            var customers = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted).ToListAsync();
            customers.ForEach(this.Deserialize);
            return customers;
        }

        public async Task<IList<Customer>> FindAllIncludeDeleted()
        {
            var customers = await this.TenantFilteredEntitiesQuery.ToListAsync();
            customers.ForEach(this.Deserialize);
            return customers;
        }

        public async Task<IList<Customer>> FindFromWildcard(string wildcard)
        {
            var predicate = BuildCombinedWildcardPredicate(wildcard);
            var customers = await AppDbContext.Customers.Where(x => x.TenantId == this.TenantId && !x.IsDeleted).Where(predicate).Include(x => x.CustomerGroupCustomers).ToListAsync();

            customers.ForEach(Deserialize);

            //HACK: For some reason, including account takes a long time. Så this is quicker
            //therefore account must be loaded manually
            await LoadAccountsForCustomers(customers.Select(x => x.Id));

            return customers;
        }
       
        private async Task LoadAccountsForCustomer(Guid customerId)
        {
            await AppDbContext.Accounts.Where(x => x.CustomerId == customerId).LoadAsync();
        }

        private async Task LoadAccountsForCustomers(IEnumerable<Guid> customerIds)
        {
            await AppDbContext.Accounts.Where(x => customerIds.Contains(x.CustomerId)).LoadAsync();
        }

        private Expression<Func<Customer, bool>> BuildCombinedWildcardPredicate(string wildcard)
        {
            var wildcards = wildcard.Split(' ');
            var combinedPredicate = wildcards.Select(BuildCustomerWildcardPredicate).Aggregate((prod, next) => prod.And(next)).Or(BuildCustomerWildcardPredicate(wildcard));
            return combinedPredicate;
        }

        private Expression<Func<Customer, bool>> BuildCustomerWildcardPredicate(string wildcard)
        {
            var parsedSuccess = int.TryParse(wildcard, out var parsedWildcard);
            var predicate = PredicateBuilder.Create<Customer>(x =>
                    (!x.IsDeleted) && (
                    (x.Fullname != null && x.Fullname.Firstname != null &&
                     x.Fullname.Firstname.ToLower().Contains(wildcard.ToLower())) ||
                    (x.Fullname != null && x.Fullname.Lastname != null &&
                     x.Fullname.Lastname.ToLower().Contains(wildcard.ToLower())) ||
                    (x.Att != null && x.Att.Firstname != null &&
                     x.Att.Firstname.ToLower().Contains(wildcard.ToLower())) ||
                    (x.Att != null && x.Att.Lastname != null &&
                     x.Att.Lastname.ToLower().Contains(wildcard.ToLower())) ||
                    (x.CompanyName != null && x.CompanyName.ToLower().Contains(wildcard.ToLower())) ||
                    (x.Cvr != null && x.Cvr.ToLower().Contains(wildcard.ToLower())) ||
                    (parsedSuccess && x.FriendlyId == parsedWildcard)) ||
                    (x.EmailEntriesJson != null &&
                     x.EmailEntriesJson.ToLower().Contains(("Email\":\"" + wildcard).ToLower())) ||
                    (x.PhoneEntriesJson != null &&
                    x.PhoneEntriesJson.ToLower().Contains(("Number\":\"" + wildcard).ToLower()))
                );

            return predicate;
        }

        public void Update(Customer modifiedCustomer)
        {
            modifiedCustomer.PhoneEntries = modifiedCustomer.PhoneEntries.Where(x => !x.IsDeleted).ToList();
            modifiedCustomer.EmailEntries = modifiedCustomer.EmailEntries.Where(x => !x.IsDeleted).ToList();
            modifiedCustomer.LicensePlateEntries = modifiedCustomer.LicensePlateEntries.Where(x => !x.IsDeleted).ToList();
            modifiedCustomer.FamilyMembers = modifiedCustomer.FamilyMembers.Where(x => !x.IsDeleted).ToList();
            this.Serialize(modifiedCustomer);

            this.AppDbContext.RemoveRange(modifiedCustomer.CustomerGroupCustomers.Where(x => x.IsDeleted));
            this.AppDbContext.Update(modifiedCustomer);
        }

        private void Serialize(Customer customer)
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            customer.EmailEntriesJson = customer.EmailEntries.Count == 0 ? null : JsonConvert.SerializeObject(customer.EmailEntries, jsonSerializerSettings);
            customer.PhoneEntriesJson = customer.PhoneEntries.Count == 0 ? null : JsonConvert.SerializeObject(customer.PhoneEntries, jsonSerializerSettings);

            customer.LicensePlateEntries.ForEach(x => x.Number = x.Number?.Replace("-", ""));
            customer.LicensePlateEntriesJson = customer.LicensePlateEntries.Count == 0 ? null :  JsonConvert.SerializeObject(customer.LicensePlateEntries, jsonSerializerSettings);

            customer.FamilyMembersJson = customer.FamilyMembers.Count == 0 ? null : JsonConvert.SerializeObject(customer.FamilyMembers, jsonSerializerSettings);
        }

        private void Deserialize(Customer customer)
        {
            customer.EmailEntries = customer.EmailEntriesJson == null ? new List<EmailEntry>() : JsonConvert.DeserializeObject<List<EmailEntry>>(customer.EmailEntriesJson);
            customer.PhoneEntries = customer.PhoneEntriesJson == null ? new List<PhoneEntry>() : JsonConvert.DeserializeObject<List<PhoneEntry>>(customer.PhoneEntriesJson);
            
            customer.LicensePlateEntries = customer.LicensePlateEntriesJson == null ? new List<LicensePlateEntry>() : JsonConvert.DeserializeObject<List<LicensePlateEntry>>(customer.LicensePlateEntriesJson);
            customer.LicensePlateEntries.ForEach(x => x.Number = x.Number?.Replace("-", ""));
            
            customer.FamilyMembers = customer.FamilyMembersJson == null ? new List<FamilyMember>() : JsonConvert.DeserializeObject<List<FamilyMember>>(customer.FamilyMembersJson);
        }
    }
}

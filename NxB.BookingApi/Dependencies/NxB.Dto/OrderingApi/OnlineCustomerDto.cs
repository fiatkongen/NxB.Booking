using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.OrderingApi;

public class OnlineCustomerDto
{
    public string LicensePlate { get; set; }
    public string Note { get; set; }

    public string Address { get; set; }
    public string City { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string CountryId { get; set; }

    public string Zip { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Firstname { get; set; }
    public string Lastname { get; set; }

    public string Email { get; set; }
    public string Prefix { get; set; }
    public string Phone { get; set; }
    public DateTime ArrivalTime { get; set; }
}
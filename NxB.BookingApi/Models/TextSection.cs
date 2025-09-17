using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TextSection
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Text { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Keywords { get; set; }
        public TextSectionType Type { get; set; }
        public DateTime? PublishDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsRead { get; set; }
        public string VideoUrl { get; set; }
        public int? Sort { get; set; }
        public string HelpUrlMatch { get; set; }

        private TextSection() { }

        public TextSection(Guid id)
        {
            Id = id;
            CreateDate = DateTime.Now.ToEuTimeZone();
        }

        public TextSection(Guid id, TextSectionType type, string text, string title) : this(id)
        {
            Type = type;
            Text = text;
            Title = title;
        }

        public void Publish()
        {
            this.PublishDate = DateTime.Now.ToEuTimeZone();
        }

        public void Unpublish()
        {
            this.PublishDate = null;
        }

        public void Delete()
        {
            this.IsDeleted = true;
        }
    }
}
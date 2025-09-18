using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
	public class CostItemSpecificDay : CostItemSpecific
	{
		public override string Name
		{
			get
			{
				//return day.GetWeekDayName();
				return day.ToString();
			}
		}

		private DayOfWeek day;
		public DayOfWeek Day
		{
			get { return day; }
			set
			{
				day = value;
			}
		}

		public CostItemSpecificDay(DayOfWeek day, CostInterval costInterval)
			: base(costInterval)
		{
			Day = day;
			CostInterval = costInterval;
		}

		public override string ToString()
		{
			return ((int)Day).ToString();
		}
	}
}
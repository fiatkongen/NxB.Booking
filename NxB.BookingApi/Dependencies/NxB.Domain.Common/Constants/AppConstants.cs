using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Constants
{
    public static class AppConstants
    {
        public static Guid ADMINISTRATOR_ID = Guid.Parse("71667d75-5203-4b6b-9860-ec0d8c46f523");
        public static Guid SCHEDULER_ID = Guid.Parse("2f96c24b-f5e8-479c-809b-9223e5cf81fc");
        public static Guid DEMONSTRATION_TENANT_ID = Guid.Parse("37A28F72-DBC1-4B8B-A4F9-F8C146002123");
        public static Guid AUNING_TENANT_ID = Guid.Parse("5959F5E1-71C1-45A8-A5F2-3697C38F3700");
        public static Guid DOCUMENTTEMPLATE_ID_SMS_LINK = Guid.Parse("89EC625E-B232-4AB7-BD96-97330D247650");
        public static Guid DOCUMENTTEMPLATE_SMS_ID_GUEST_HAS_ONLINEBOOKED = Guid.Parse("42E678B0-B75F-46FB-9566-5D772FC304B6");
        public static Guid DOCUMENTTEMPLATE_KIOSK_PAYMENT = Guid.Parse("F3FC0FB6-2DCC-4A71-895A-AE4EE18569F9");
        public static Guid MAPVIEW_ID = Guid.Parse("9DD0575A-B325-477E-A596-1360E0C6BBF4");

        public static string SCHEDULER_NAME = "scheduler";
        public static string NXB_FILE_STORAGEROOT = "https://nxbfilestorage.blob.core.windows.net";

        public static string AZURE_UPLOAD_CONTAINER = "documentimages";
        public static string AZURE_UPLOAD_ONLINE_SECTION_IMAGES = "onlinesectionimages";

        public static Guid ONLINEUSER_ID = Guid.Parse("55864d24-2ae5-42af-a8dd-f17935a1d861");
        public static string ONLINEUSER_NAME = "OnlineUser";
        public const string DEFAULT_ACCOUNT_NAME = "Standard";

        public static string AZURE_CONTAINER_QRCODES = "qrcodes";
        

        public static int LONG_RUNNING_SQL_TIMEOUT_SECONDS = 60;
        
        public static string CACHE_COST_UPDATED = "cache_cost_updated";

        public static Guid FEATUREMODULEID_GUESTINFO = Guid.Parse("B6702C88-AC71-424D-9A45-EFE9711B0649");

        public static string GetPdfFileStorageUrl(Guid fileId)
        {
            return NXB_FILE_STORAGEROOT + "/pdf/" + fileId;
        }

        public static string GetFileStorageUrl(string container, Guid fileId)
        {
            return NXB_FILE_STORAGEROOT + $"/{container}/{fileId}";
        }

        public static string GetImageSectionFileStorageUrl(Guid fileId)
        {
            return NXB_FILE_STORAGEROOT + "/onlinesectionimages/" + fileId;
        }

        public static string GetGuestInfoUrlForBooking(Guid bookingId, Guid tenantId)
        {
            return $"https://guest.next-stay-booking.dk/{bookingId}?tenantId={tenantId}";
        }

        public static string GetMapViewUrlForBooking(Guid bookingId, Guid tenantId)
        {
            return $"https://guest.next-stay-booking.dk/map-view/{bookingId}?tenantId={tenantId}";
        }
    }
}

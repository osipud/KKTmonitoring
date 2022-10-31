using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KKT_STATUS_WEB.Models
{
    public class Device
    {
        public int DeviceId { get; set; }
        public string? FiscalStoreNumber { get; set; }
        public int? SpentResource { get; set; }
        public DateTime LastDocumentDate { get; set; }
        public int LastDocumentNumber { get; set; }
        public int ResidualResource { get; set; }
        public double ResidualProcent { get; set; }
        public string? SoftwareVersion { get; set; }
        public double LastDocumentDateTimeSpan { get; set; }
        public bool FiscalStoreStatus { get; set; }
    }
        /*
        +DeviceId - инкрементный номер устройства 
        +FiscalStoreNumber - номер фискального накопителя
        +SpentResource - потраченный ресурс фискального накопителя
        +LastDocumentDate - дата последнего отфискализированного документа
        +LastDocumentNumber - номер последнего отфискализированного документа
        ResidualResource - остаток ресурсов фискального накопителя
        ResidualProcent - процент остатка фискального ресурса
        SoftwareVersion - версия программного обеспечения
        LastDocumentDateTimeSpan - диапозон разницы между текущим временем и датой последнего документа
        FiscalStoreStatus - значение присутствия фискального накопителя в устройстве 
        */
}

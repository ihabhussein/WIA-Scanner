using System;
using System.Collections.Generic;
using WIA;

class WIAScanner {
    private enum WIA_PROPERTIES: uint {
        WIA_RESERVED_FOR_NEW_PROPS = 1024,

        WIA_DIP_FIRST = 2,
        WIA_DIP_DEV_NAME = WIA_DIP_FIRST + 5,

        WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS,

        WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS,

        WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS,
        WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13,
        WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14
    }

	[Flags]
    private enum WIA_DPS_DOCUMENT_HANDLING_STATUS: uint {
        FEED_READY = 0x00000001
    }

	[Flags]
    private enum WIA_DPS_DOCUMENT_HANDLING_SELECT: uint {
        FEEDER = 0x00000001,
        FLATBED = 0x00000002
    }

    public static Dictionary<string, string> ListDevices() {
        Dictionary<string, string> devices = new Dictionary<string, string>();
        DeviceManager manager = new DeviceManager();
        foreach (DeviceInfo info in manager.DeviceInfos) {
        	foreach (Property p in info.Properties) {
        		if (p.PropertyID == (uint)WIA_PROPERTIES.WIA_DIP_DEV_NAME) devices.Add(info.DeviceID, p.get_Value().ToString());
        	}
        }
        return devices;
    }

    public static string SelectDevice() {
        ICommonDialog dialog = new CommonDialog();
        Device device = dialog.ShowSelectDevice(WiaDeviceType.UnspecifiedDeviceType, true, false);
        return (device == null)? string.Empty: device.DeviceID;
    }

    public static Device SelectDevice(string deviceId) {
        DeviceManager manager = new DeviceManager();
        Device device = null;
        foreach (DeviceInfo info in manager.DeviceInfos) {
            if (info.DeviceID == deviceId) {
                device = info.Connect();
                break;
            }
        }
        if (device == null) throw new ArgumentOutOfRangeException("deviceId", "Incorrect device ID");
        return device;
    }

    public static List<string> Scan(string deviceId) {
        ICommonDialog wiaCommonDialog = new CommonDialog();
    	List<string> fileNames = new List<string>();
   		bool hasSheetFeeder = false;
        bool hasMorePages = false;

        do {
        	Device device = SelectDevice(deviceId);
            ImageFile image = null;

            try {
            	image = (ImageFile)wiaCommonDialog.ShowAcquireImage();
                if (image != null) {
	                string fileName = System.IO.Path.GetTempFileName();
	                System.IO.File.Delete(fileName);
	                image.SaveFile(fileName);
	                fileNames.Add(fileName);
                }
            } finally {
                image = null;

                foreach (Property prop in device.Properties) {
		        	if (prop.PropertyID == (uint)WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_SELECT)
		        		hasSheetFeeder = ((WIA_DPS_DOCUMENT_HANDLING_SELECT)prop.get_Value() == WIA_DPS_DOCUMENT_HANDLING_SELECT.FEEDER);
		
		        	if (prop.PropertyID == (uint)WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_STATUS)
                		hasMorePages = ((WIA_DPS_DOCUMENT_HANDLING_STATUS)prop.get_Value() == WIA_DPS_DOCUMENT_HANDLING_STATUS.FEED_READY);
                }
            }
        } while (hasSheetFeeder && hasMorePages);

        return fileNames;
    }
}
	
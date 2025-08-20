# Windows Service Setup

This slideshow application can be installed as a Windows service to run automatically in the background, even when no user is logged in.

## Project Structure

```
slideshow/
├── app/                    # Main slideshow application
│   ├── server.js          # Web server
│   ├── index.html         # Slideshow interface
│   ├── script.js          # Client-side logic
│   ├── styles.css         # Styling
│   ├── config.json        # App configuration
│   ├── package.json       # App dependencies
│   └── README.md          # App documentation
└── service/               # Windows service management
    ├── service.js         # Service management script
    ├── package.json       # Service dependencies
    ├── install-service.bat
    ├── uninstall-service.bat
    ├── start-service.bat
    ├── stop-service.bat
    └── WINDOWS_SERVICE.md # This file
```

## Prerequisites

1. **Node.js** must be installed on the Windows machine
2. **Administrator privileges** are required for service installation
3. The slideshow files must be in a permanent location (not on Desktop or temporary folders)

## Installation Steps

### 1. Install All Dependencies
From the project root, install both app and service dependencies:
```bash
npm run setup-all
```

### 2. Install as Windows Service
Run **as Administrator** (right-click Command Prompt/PowerShell and select "Run as administrator"):

**Option A: Using npm script (recommended)**
```bash
npm run install-service
```

**Option B: Using batch file**
```bash
# Navigate to service folder and double-click:
service\install-service.bat
```

**Option C: Using Node directly**
```bash
cd service
node service.js install
```

### 3. Verify Installation
The service will be installed as "Slideshow Server" and should start automatically. You can verify it's running by:
- Opening Windows Services (services.msc)
- Looking for "Slideshow Server" in the list
- Checking that its status is "Running"

## Service Management

All service management can be done from the project root:

### Start the Service
```bash
npm run start-service
# or use batch file: service\start-service.bat
```

### Stop the Service
```bash
npm run stop-service
# or use batch file: service\stop-service.bat
```

### Restart the Service
```bash
npm run restart-service
```

### Uninstall the Service
```bash
npm run uninstall-service
# or use batch file: service\uninstall-service.bat
```

## Service Configuration

The service will:
- **Run automatically** when Windows starts
- **Continue running** even when no user is logged in
- **Restart automatically** if it crashes
- **Use the config.json** settings from the `app` folder
- **Serve the slideshow** at `http://localhost:3000`

To modify slideshow settings, edit `app/config.json` and restart the service.

## Accessing the Slideshow

Once the service is running, you can access the slideshow by:
1. Opening a web browser on the same machine
2. Going to `http://localhost:3000`
3. The slideshow will start automatically (if `autoStart` is enabled in config.json)

## Remote Access

To access the slideshow from other devices on the network, you may need to:
1. Configure Windows Firewall to allow connections on port 3000
2. Access using the machine's IP address: `http://[MACHINE_IP]:3000`

## Troubleshooting

### Service Won't Install
- Make sure you're running as Administrator
- Check that Node.js is properly installed
- Verify that `npm install` completed successfully

### Service Won't Start
- Check Windows Event Viewer for error messages
- Verify that the config.json file exists and is valid
- Ensure the configured folder path exists and is accessible

### Can't Access Slideshow
- Verify the service is running in Windows Services
- Check that port 3000 isn't being used by another application
- Try accessing `http://localhost:3000` directly on the server machine

## Service Logs

Service logs can be found in Windows Event Viewer:
1. Open Event Viewer (eventvwr.msc)
2. Navigate to "Windows Logs" > "Application"
3. Look for events from "Slideshow Server"

## File Locations

Keep these files in the service directory:
- `server.js` - Main application
- `config.json` - Configuration file
- `service.js` - Service management script
- All web files (HTML, CSS, JS)

**Important**: Don't move or delete these files once the service is installed, as the service references the exact file paths.

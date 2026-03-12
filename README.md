# DMY Member Monitor Application

A comprehensive Windows-based member monitoring and management system designed for enterprise environments. This application provides real-time monitoring, remote control, and administrative capabilities for managed client systems.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Components](#components)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Building](#building)
- [Installation](#installation)
- [Configuration](#configuration)
- [Communication Protocol](#communication-protocol)
- [License](#license)

## Overview

The DMY Member Monitor Application is a client-server monitoring solution that enables administrators to monitor and manage multiple Windows workstations from a central server. The client agent runs on monitored machines and collects various data points including process information, network activity, file operations, and more.

## Architecture

The system follows a three-tier architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                      Server Layer                           │
│  (INMServer - Management Console)                           │
│  (INMService - Background Service)                          │
│  (DBManager - Database Management)                          │
└─────────────────────────────────────────────────────────────┘
                              │
                    TCP/IP Communication
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Communication Library                     │
│  (INMC - Custom TCP Messaging Framework)                    │
└─────────────────────────────────────────────────────────────┘
                              │
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Client Layer                             │
│  (INMAgent - Monitoring Agent)                              │
└─────────────────────────────────────────────────────────────┘
```

## Components

### Agent (Client-Side)

| Component | Description |
|-----------|-------------|
| **INMAgent** | Main client application with UI and monitoring logic |
| **AgentEngine** | Core engine for tracking and monitoring |
| **AgentService** | Windows service implementation |
| **AgentSetup** | Installation package |
| **AgentWebInet** | Web integration component |

### Server (Management)

| Component | Description |
|-----------|-------------|
| **INMServer** | Main server management console (Windows Forms) |
| **INMService** | Background service for server operations |
| **DBManager** | Database backup and restore management |

### Communication Library (CommLib/INMC)

Custom TCP-based communication framework providing:

- **Client/Server** - Connection-oriented messaging
- **Protocols** - Binary serialization for data exchange
- **Messengers** - Request/Reply and Synchronized messaging
- **Services** - Remote method invocation support

## Features

### Monitoring Capabilities

- **Process Monitoring** - Track running applications and processes
- **Network Monitoring** - Monitor network connections and bandwidth
- **URL/Website History** - Record browsing activity
- **File Operations** - Track file create/modify/delete operations
- **Print History** - Monitor printing activities
- **Device Changes** - Detect hardware changes
- **Application Usage** - Track application usage patterns
- **Screen Capture** - Real-time screenshots
- **Installation Tracking** - Monitor software installations

### Administrative Controls

- **Remote Lock** - Lock mouse and keyboard
- **Shutdown/Reboot** - Remote system shutdown or restart
- **Message Send** - Send messages to clients
- **User Management** - Create/modify/delete users
- **Application Blocking** - Prohibit specific applications
- **Network Filtering** - Allow/block network access
- **Port Management** - Firewall port rules
- **Bandwidth Control** - Set bandwidth limits

### Database

- **SQLite** - Local data storage (both 32-bit and 64-bit)
- **MySQL** - Centralized database support

## Technology Stack

| Layer | Technology |
|-------|------------|
| **Language** | C# (.NET Framework) |
| **UI** | Windows Forms |
| **Communication** | TCP/IP (Custom INMC Protocol) |
| **Database** | SQLite, MySQL |
| **Build System** | MSBuild / Visual Studio |
| **Target OS** | Windows XP/Vista/7+ |

## Project Structure

```
dmy-member-monitor-app/
├── Agent/                      # Client-side monitoring agent
│   ├── INMAgent/              # Main agent application
│   ├── AgentEngine/           # Core tracking engine
│   │   ├── COMMON/           # Common utilities
│   │   ├── GDI/              # Graphics utilities
│   │   ├── HOOK/             # Keyboard/Mouse hooks
│   │   ├── TRACK/            # Tracking modules
│   │   │   └── DismountUSB/  # USB device management
│   │   └── URL/               # URL history
│   ├── AgentService/          # Windows service
│   ├── AgentSetup/           # Installer
│   ├── AgentWebInet/         # Web integration
│   └── INMC/                 # Communication library
│
├── Manager/                    # Server-side components
│   ├── INMServer/            # Main server console
│   ├── INMService/           # Background service
│   └── DBManager/            # Database management
│
├── Component/                 # Third-party components
│   ├── SQLite32/             # 32-bit SQLite
│   └── SQLite64/             # 64-bit SQLite
│
├── Database/                  # Database scripts
│   └── inms2db_essential.sql
│
└── README.md
```

## Building

### Prerequisites

- Visual Studio 2010 or later
- .NET Framework 3.5 or higher
- Windows SDK (for Windows Forms)

### Build Instructions

1. Open the solution file:
   - `INMAgent1.20(vs2010).sln` for Agent
   - `INManager1.20(vs2010).sln` for Manager

2. Select the appropriate build configuration:
   - **Debug** - Development build
   - **Release** - Production build

3. Build using MSBuild:
   ```bash
   msbuild INMAgent.sln /p:Configuration=Release
   ```

4. Or open in Visual Studio:
   ```bash
   start INMAgent1.20(vs2010).sln
   ```

### Build Outputs

- **Agent**: `INMAgent/bin/Release/INMAgent.exe`
- **Server**: `INMServer/bin/Release/INMServer.exe`
- **Service**: `INMService/bin/Release/INMService.exe`

## Installation

### Server Installation

1. Run `INMServer.exe` on the management server
2. Configure database connection settings
3. Set up administrator accounts
4. Configure network ports (default: 10180-10182)

### Client Installation

1. Run `AgentSetup.exe` on target machines
2. Configure server address via registry or setup wizard
3. The agent will start automatically on system boot

### Registry Configuration

The agent reads server configuration from:
```
HKEY_LOCAL_MACHINE\SOFTWARE\HanaYonghe Inc\IMS Agent
```

- `ManAddress` - Server IP address (default: 192.168.3.168)

## Configuration

### Server Ports

| Port | Purpose |
|------|---------|
| 10180 | Main communication |
| 10181 | Background communication |
| 10182 | Initialization |

### Timer Intervals

Default polling intervals (in seconds):
- Process check: 2.3s
- State update: 1s
- History: 1s
- Screen capture: 500ms
- Web URL history: 80s
- Network app history: 100s

## Communication Protocol

### Message Types

The system uses custom message types (MSG0-MSG8) for different operations:

- **MSG0**: Agent information
- **MSG1**: Prohibit settings / App disable
- **MSG2**: Network settings / App monitoring
- **MSG3**: Firewall / Port settings
- **MSG4**: Real-time app/screen monitoring
- **MSG5**: File/Print/Mail history
- **MSG6**: Control (lock/shutdown/users)
- **MSG7**: Web/Network history
- **MSG8**: Device/Process changes

### Data Serialization

- Uses .NET BinaryFormatter for message serialization
- Messages sent via TCP/IP using custom INMC protocol
- Both raw data and text message formats supported

## Database Schema

The essential database schema is stored in `inms2db_essential.sql`. Key tables include:

- Agent information
- Process history
- Network connections
- File operations
- Print logs
- Device changes
- User activities

## Error Handling

- Errors logged to `CustsErr.log` in application directory
- Uses Windows Event Log for critical errors
- Network disconnection auto-reconnect (6-second interval)

## Version Information

- **Version**: 1.20
- **Visual Studio**: 2010
- **Target Framework**: .NET

## License

This software is proprietary and may only be used with proper licensing. Contact the vendor for license information.

---

For technical support and documentation, please refer to the vendor documentation or contact your system administrator.


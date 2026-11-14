# SharpChat

A real-time chat application built with ASP.NET Core SignalR and Blazor WebAssembly. SharpChat provides secure, real-time messaging with support for private conversations and message persistence.

## Features

- **Real-time messaging** using ASP.NET Core SignalR
- **Private chat rooms** with one-to-one messaging
- **Blazor WebAssembly client** for a rich, interactive UI
- **Offline message persistence** using IndexedDB
- **Message delivery status** (Pending, Sent, Delivered)
- **User authentication** with custom user identification
- **Responsive design** built with modern Blazor components

## Architecture

SharpChat follows a clean, modular architecture with three main components:

### 1. **SharpChat.Client** (Blazor WebAssembly)
- Frontend application built with Blazor WebAssembly
- Real-time SignalR client connections
- IndexedDB for offline message storage
- Responsive UI components

### 2. **SharpChat.Server** (ASP.NET Core Web API)
- SignalR hubs for real-time communication
- REST API endpoints
- User authentication and management
- Message routing and delivery

### 3. **Common** (Shared Library)
- Shared data models and DTOs
- Cross-platform business logic
- Enums and common utilities

## Technology Stack

- **.NET 10.0** - Cross-platform runtime
- **ASP.NET Core** - Web framework
- **Blazor WebAssembly** - Client-side UI framework
- **SignalR** - Real-time communication
- **IndexedDB** - Client-side storage (via Tavenem.Blazor.IndexedDB)
- **Razor Components** - UI component framework

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/) (optional)
- Modern web browser (Chrome, Edge, Firefox, Safari)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/SharpChat.git
   cd SharpChat
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/SharpChat.Server
   ```

4. **Access the application**
   - Open your browser and navigate to `https://localhost:7049`
   - The client application will be served automatically

### Development Workflow

1. **Start both projects simultaneously**
   - In Visual Studio/Rider: Set both `SharpChat.Server` and `SharpChat.Client` as startup projects
   - Or use the provided `.sln` file

2. **Test the SignalR connection**
   - The application includes HTTP test files at `src/SharpChat.Server/MessengerServer.http`
   - Use these to test API endpoints and SignalR hubs

## Project Structure

```
SharpChat/
├── src/
│   ├── Common/                    # Shared library
│   │   ├── ChatMessage.cs         # Message data model
│   │   ├── SignalMessage.cs       # Signaling messages
│   │   └── Common.csproj
│   ├── SharpChat.Client/          # Blazor WebAssembly client
│   │   ├── Pages/                 # Razor pages
│   │   ├── Services/              # Client services
│   │   ├── Storage/               # IndexedDB integration
│   │   ├── Models/                # Client-side models
│   │   ├── Layout/                # Layout components
│   │   ├── wwwroot/               # Static assets
│   │   └── Messenger.BlazorClient.csproj
│   └── SharpChat.Server/          # ASP.NET Core server
│       ├── Hubs/                  # SignalR hubs
│       │   ├── ChatHub.cs         # Chat functionality
│       │   └── SignalingHub.cs    # WebRTC signaling
│       ├── Properties/            # Server properties
│       ├── appsettings.json       # Configuration
│       └── MessengerServer.csproj
├── SharpChat.sln                  # Solution file
└── README.md                      # This file
```

## SignalR Hubs

### ChatHub
Handles real-time chat functionality:
- `SendPrivateMessage(targetUser, message)` - Send private messages between users
- Automatic message delivery status tracking
- User connection management

### SignalingHub
Manages WebRTC signaling for potential future features:
- Peer-to-peer connection establishment
- ICE candidate exchange
- Session description protocol (SDP) handling

## Storage & Persistence

### Client-Side (IndexedDB)
- Messages are stored locally using IndexedDB
- Offline message queuing
- Automatic synchronization when online
- Data encryption for privacy

### Server-Side
- In-memory user connection tracking
- Message routing without persistent storage (stateless design)
- Scalable architecture for future database integration

## Configuration

### Server Configuration (`appsettings.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Client Configuration
- SignalR connection URL: Configured in `Services/HubConnectionService.cs`
- IndexedDB settings: Defined in `Storage/IndexedDbConfig.cs`
- API endpoints: Configured via dependency injection

## Development Notes

### Custom User Identification
The server includes a `CustomUserIdProvider.cs` that extracts user identifiers from SignalR connections, allowing for custom authentication schemes.

### Message Status Flow
1. **Pending** - Message created locally, awaiting delivery
2. **Sent** - Message successfully sent to server
3. **Delivered** - Message delivered to recipient

### Testing
- Use the included `MessengerServer.http` file for API testing
- SignalR hubs can be tested using the Blazor client or third-party tools
- IndexedDB functionality works in modern browsers with developer tools

## Future Enhancements

Planned features and improvements:

- **Group chats** with multiple participants
- **File sharing** with upload/download capabilities
- **Message encryption** for enhanced security
- **Voice/video calling** using WebRTC
- **Message history** with server-side storage
- **User presence** indicators (online/offline/away)
- **Push notifications** for new messages
- **Theming** support (light/dark mode)
- **Mobile applications** using .NET MAUI

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet)
- Real-time functionality powered by [SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)
- UI components based on [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- Client-side storage using [Tavenem.Blazor.IndexedDB](https://github.com/Tavenem/Tavenem.Blazor.IndexedDB)

## Support

For issues, questions, or feature requests:
- Open an issue on GitHub
- Check the project documentation
- Review existing issues for similar problems

---

**Happy chatting!** 🚀
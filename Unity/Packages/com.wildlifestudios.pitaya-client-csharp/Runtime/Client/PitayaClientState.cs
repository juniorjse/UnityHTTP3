
namespace Wildlife.PitayaCSharp.Client
{
    public enum PitayaClientState
    {
        Inited = 0, //Waiting for the player to start a connection
        Connecting = 1,
        Connected = 2,
        Disconnecting = 3,
        Unknown = 4 // you should discart the client and create a new instance
    }

}
using System;
using System.IO;
using Wildlife.PitayaCSharp.Client;
using Wildlife.PitayaCSharp.Protocol;
using Wildlife.PitayaCSharp.Error;

namespace Wildlife.PitayaCSharp.Transport
{
    public class TransporterPluginRepository
    {
        static readonly TransporterPluginRepository _instance = new TransporterPluginRepository();
        public static TransporterPluginRepository Instance => _instance;
        private TransporterPluginRepository() { }
        public const uint TransporterPluginSlotCount = 8;
        public ITransporterPlugin[] plugins = new ITransporterPlugin[TransporterPluginSlotCount];

        public void Register(ITransporterPlugin plugin)
        {
            byte transporterName;

            if (plugin == null || (byte)plugin.Name >= TransporterPluginSlotCount || (byte)plugin.Name < 0)
                throw new PitayaInvalidArgumentException();

            transporterName = (byte)plugin.Name;
            if (plugins[transporterName] != null)
                Deregister((TransporterName)transporterName);

            plugins[transporterName] = plugin;

            plugin.OnRegister();
        }

        public void Deregister(TransporterName transporterName)
        {
            byte transName = (byte)transporterName;

            if (transName >= TransporterPluginSlotCount || transName < 0)
                throw new PitayaInvalidArgumentException();

            ITransporterPlugin transporterPlugin = plugins[transName];

            if (transporterPlugin != null)
                transporterPlugin.OnDeregister();

            plugins[transName] = null;
        }

        public ITransporterPlugin Get(TransporterName transporterName)
        {
            byte transName = (byte)transporterName;
            if (transName >= TransporterPluginSlotCount || transName < 0)
                return null;

            return plugins[transName];
        }
    }
}
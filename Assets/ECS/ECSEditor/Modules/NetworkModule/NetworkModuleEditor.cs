#if STATES_HISTORY_MODULE_SUPPORT && NETWORK_MODULE_SUPPORT
namespace ME.ECSEditor {

    using ME.ECS;
    using ME.ECS.Network;
    
    [ComponentCustomEditor(typeof(INetworkModuleBase))]
    public class NetworkModuleEditor : ME.ECSEditor.IGUIEditor<INetworkModuleBase> {

        public INetworkModuleBase target { get; set; }

        public T GetTarget<T>() {

            return (T)(object)this.target;

        }

        bool IGUIEditorBase.OnDrawGUI() {
                
            var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label);
            style.richText = true;

            var rpcCount = this.target.GetRegistryCount().ToString();
            var ping = this.target.GetPing().ToString("###0.000ms");
            var eventsSent = this.target.GetEventsSentCount().ToString();
            
            var eventsBytesSent = this.target.GetEventsBytesSentCount();
            var eventsBytesSentStr = MathUtils.BytesCountToString(eventsBytesSent);

            var eventsBytesReceived = this.target.GetEventsBytesReceivedCount();
            var eventsBytesReceivedStr = MathUtils.BytesCountToString(eventsBytesReceived);

            UnityEngine.GUILayout.Label("<b>Registered RPCs:</b> " + rpcCount, style);
            UnityEngine.GUILayout.Label("<b>Ping:</b> " + ping, style);
            UnityEngine.GUILayout.Label("<b>Events Sent: </b>" + eventsSent, style);
            UnityEngine.GUILayout.Label("<b>Sent: </b>" + eventsBytesSentStr, style);
            UnityEngine.GUILayout.Label("<b>Received: </b>" + eventsBytesReceivedStr, style);

            return false;

        }

    }

}
#endif
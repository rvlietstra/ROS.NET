﻿#region USINGZ

using Messages;
using m = Messages;
using gm = Messages.geometry_msgs;
using nm = Messages.nav_msgs;

#endregion

namespace EricIsAMAZING
{
    public class SubscriptionCallbackHelper<M> : ISubscriptionCallbackHelper where M : IRosMessage, new()
    {
        public ParameterAdapter<M> Adapter = new ParameterAdapter<M>();

        public SubscriptionCallbackHelper(M msg)
        {
            type = msg.type;
        }

        public SubscriptionCallbackHelper(CallbackQueueInterface q)
            : base(q)
        {
        }
    }

    public class ISubscriptionCallbackHelper
    {
        private CallbackQueueInterface Callback;
        public TypeEnum type = TypeEnum.Unknown;

        protected ISubscriptionCallbackHelper()
        {
        }

        protected ISubscriptionCallbackHelper(CallbackQueueInterface Callback)
        {
            this.Callback = Callback;
        }

        public virtual byte[] deserialize(SubscriptionCallbackHelperDeserializeParams parms)
        {
            return null;
        }

        public virtual void call(SubscriptionCallbackHelperCallParams parms)
        {
        }

        public virtual TypeEnum getTypeInfo()
        {
            return type;
        }

        public virtual bool isConst()
        {
            return true;
        }
    }

    public class SubscriptionCallbackHelperDeserializeParams
    {
        public byte[] buffer;
        public string connection_header;
        public int length;
    }

    public class SubscriptionCallbackHelperCallParams
    {
        public IMessageEvent Event;
    }

    public class ParameterAdapter<P> : IParameterAdapter where P : IRosMessage
    {
    }

    public abstract class IParameterAdapter
    {
    }

    public class MessageStuff<T>
    {
    }
}
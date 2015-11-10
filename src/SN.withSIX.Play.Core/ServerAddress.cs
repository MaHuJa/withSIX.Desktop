using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Play.Core
{
    [Obsolete("BWC for datacontract madness")]
        // This is IPEndPoint really?
        [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
        public class ServerAddress : IEquatable<ServerAddress>
        {
            [DataMember]
            IPAddress _ip;
            [DataMember]
            int _port;
            string _stringFormat;

            public ServerAddress(string address)
            {
                var addrs = address.Split(':');
                if (addrs.Length < 2)
                    throw new Exception("Invalid address format: " + address);

                var port = TryInt(addrs.Last());
                if (port < 1 || port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException(port.ToString());

                _ip = IPAddress.Parse(string.Join(":", addrs.Take(addrs.Length - 1)));
                _port = port;
                _stringFormat = GetStringFormat();
            }

            public ServerAddress(string ip, int port)
            {
                if (string.IsNullOrWhiteSpace(ip))
                    throw new ArgumentNullException("ip");
                if (port < 1 || port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException(port.ToString());

                _ip = IPAddress.Parse(ip);
                _port = port;

                _stringFormat = GetStringFormat();
            }

            public ServerAddress(IPAddress ip, int port)
            {
                if (ip == null)
                    throw new ArgumentNullException("ip");
                if (port < 1 || port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException(port.ToString());

                _ip = ip;
                _port = port;

                _stringFormat = GetStringFormat();
            }

            public IPAddress IP
            {
                get { return _ip; }
            }
            public int Port
            {
                get { return _port; }
            }

            public bool Equals(ServerAddress other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                return ReferenceEquals(this, other) || String.Equals(_stringFormat, other._stringFormat);
            }

            string GetStringFormat()
            {
                return String.Format("{0}:{1}", IP, Port);
            }

            public override int GetHashCode()
            {
                return (_stringFormat != null ? _stringFormat.GetHashCode() : 0);
            }

            static int TryInt(string val)
            {
                int result;
                return Int32.TryParse(val, out result) ? result : 0;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                return obj.GetType() == GetType() && Equals((ServerAddress)obj);
            }

            [OnDeserialized]
            public void OnDeserialized(StreamingContext context)
            {
                if (_ip == null)
                    throw new Exception("IP cant be null");
                if (_port < 1 || _port > IPEndPoint.MaxPort)
                    throw new ArgumentOutOfRangeException(_port.ToString());

                _stringFormat = GetStringFormat();
            }

            public override string ToString()
            {
                return _stringFormat;
            }
        }

    public static class SAStuff
    {
        public static ServerAddress GetAddy(string address)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(address));
            Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(address));

            var addrs = address.Split(':');
            return
                new ServerAddress(String.Format("{0}:{1}",
                    GetValidIp(string.Join(":", addrs.Take(addrs.Length - 1))), addrs.Last().TryInt()));
        }

        public static bool IsValidIp(string ip)
        {
            IPAddress parsedIp;
            return IPAddress.TryParse(ip, out parsedIp);
        }

        public static IPAddress GetValidIp(string ip)
        {
            IPAddress parsedIp;
            if (IPAddress.TryParse(ip, out parsedIp))
                return parsedIp;

            var i = Dns.GetHostAddresses(ip).FirstOrDefault();
            if (i == null)
                throw new NullReferenceException("i");
            return i;
        }
    }
}

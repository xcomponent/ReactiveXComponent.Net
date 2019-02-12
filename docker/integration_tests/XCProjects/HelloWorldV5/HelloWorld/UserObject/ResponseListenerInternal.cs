using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using XComponent.Shared.Failover;

namespace XComponent.HelloWorld.UserObject
{
    // ICloneable interface can be implemented to override clone behaviour of XComponent
    // IDisposable interface can be implemented to dispose public/internal members when the state machine is in a final state
    // ToString() can be overridden. XCSpy uses it to display state machine instances
    [System.Serializable()]
    public class ResponseListenerInternal : IFailoverAwareStateHolder
    {
        private const string SqlConnectionStatePath = "SqlConnectionState";

        private const string SqlConnectionString = @"Data Source=(localdb)\Electryon;Initial Catalog=Electryon";

        private SqlConnection _connection;

        public void ApplyEvent(UserDataChanged userDataChanged)
        {
            //var @event = userDataChanged.Event;
            //var sqlConnectionChanged = @event as ConnectionStateChanged;
            //if (sqlConnectionChanged != null)
            //{
            //    var stringEncoding = sqlConnectionChanged.SqlConnectionState;
            //    if (stringEncoding == null)
            //    {
            //        _connection = null;
            //    }
            //    else
            //    {
            //        ConnectionState connectionState;
            //        if (Enum.TryParse(stringEncoding, out connectionState))
            //        {
            //            switch (connectionState)
            //            {
            //                case ConnectionState.Open:
            //                    OpenConnection();
            //                    break;
            //                default:
            //                    DisposeConnection();
            //                    break;
            //            }
            //        }
            //    }
            //}
        }

        public IEnumerable<object> GetNonSerializableState()
        {
            string sqlConnectionState = _connection?.State.ToString();
            return new List<object> {new ConnectionStateChanged(sqlConnectionState)};

        }

        public void DisposeNonSerializableState()
        {
            //DisposeConnection();
        }

        private void DisposeConnection()
        {
            //if (_connection != null)
            //{
            //    try
            //    {
            //        _connection.Dispose();
            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e);
            //        throw;
            //    }
            //}
        }

        public void OpenConnection()
        {
            //_connection = new SqlConnection(SqlConnectionString);
            //_connection.Open();
        }
    }
}
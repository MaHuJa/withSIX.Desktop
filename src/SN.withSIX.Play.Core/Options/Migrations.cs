// <copyright company="SIX Networks GmbH" file="Migrations.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Play.Core.Options
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core.Options")]
    public class Migrations
    {
        [DataMember] public int SyncData;
    }
}
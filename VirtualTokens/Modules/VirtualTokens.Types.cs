/*
 * Copyright (c) Contributors, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Nini.Config;

using OpenMetaverse;

using OpenSim.Services.Interfaces;
using OpenSim.Region.Framework.Scenes;

using Aurora.Framework;
using Aurora.Simulation.Base;

using OpenMetaverse.StructuredData;

namespace Aurora.Addon.VirtualTokens
{

    public class VirtualToken : Query2Type<VirtualToken>
    {
        public UUID id = UUID.Zero;
        public string code = "";
        public uint estate = 1;
        public UUID founder = UUID.Zero;
        public DateTime created;
        public UUID icon = UUID.Zero;
        public bool overridable = false;
        public UUID category = UUID.Zero;
        public string name = "";
        public string description = "";
        public bool enabled = true;

        #region Query2Type members

        public new static List<VirtualToken> doQuery2Type(List<string> query)
        {
            List<VirtualToken> tokens = new List<VirtualToken>(query.Count % 11 == 0 ? query.Count / 11 : 0);

            if (query.Count % 11 == 0)
            {
                for (int i = 0; i < query.Count; i += 11)
                {
                    uint foo;
                    bool overridable = uint.TryParse(query[i + 6], out foo) ? foo >= 1 : bool.Parse(query[i + 6]);
                    bool enabled = uint.TryParse(query[i + 8], out foo) ? foo >= 1 : bool.Parse(query[i + 8]);

                    tokens.Add(new VirtualToken
                    {
                        id = UUID.Parse(query[i]),
                        code = query[i + 1],
                        estate = uint.Parse(query[i + 2]),
                        founder = UUID.Parse(query[i + 3]),
                        created = Utils.UnixTimeToDateTime(int.Parse(query[i + 4])),
                        icon = UUID.Parse(query[i + 5]),
                        overridable = overridable,
                        category = UUID.Parse(query[i + 7]),
                        enabled = enabled,
                        name = query[i + 9].Trim(),
                        description = query[i + 10].Trim()
                    });
                }
            }

            return tokens;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap(11);

            resp["id"] = id;
            resp["code"] = code;
            resp["estate"] = estate;
            resp["founder"] = founder;
            resp["created"] = Utils.DateTimeToUnixTime(created);
            resp["icon"] = icon;
            resp["overridable"] = overridable;
            resp["category"] = category;
            resp["name"] = name;
            resp["description"] = description;
            resp["enabled"] = enabled;

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if (map.ContainsKey("id"))
            {
                id = UUID.Parse(map["id"].ToString());
            }
            if (map.ContainsKey("code"))
            {
                code = map["code"].ToString().Trim();
            }
            if (map.ContainsKey("estate"))
            {
                estate = uint.Parse(map["estate"].ToString());
            }
            if (map.ContainsKey("founder"))
            {
                founder = UUID.Parse(map["founder"].ToString());
            }
            if (map.ContainsKey("created"))
            {
                created = Utils.UnixTimeToDateTime(int.Parse(map["created"].ToString()));
            }
            if (map.ContainsKey("icon"))
            {
                icon = UUID.Parse(map["icon"].ToString());
            }
            if (map.ContainsKey("overridable"))
            {
                overridable = bool.Parse(map["overridable"].ToString());
            }
            if (map.ContainsKey("category"))
            {
                category = UUID.Parse(map["category"].ToString());
            }
            if (map.ContainsKey("name"))
            {
                name = map["name"].ToString().Trim();
            }
            if (map.ContainsKey("description"))
            {
                description = map["description"].ToString().Trim();
            }
            if (map.ContainsKey("enabled"))
            {
                enabled = bool.Parse(map["enabled"].ToString());
            }
        }

        public override bool Equals(object obj)
        {
            VirtualToken tok = obj as VirtualToken;
            return (
                tok.id == id &&
                tok.code == code &&
                tok.estate == estate &&
                tok.founder == founder &&
                tok.created.CompareTo(created) == 0 &&
                tok.icon == icon &&
                tok.overridable == overridable &&
                tok.category == category &&
                tok.name == name &&
                tok.description == description &&
                tok.enabled == enabled
            );
        }

        #endregion

        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VirtualTokenIssuer : Query2Type<VirtualTokenIssuer>
    {
        public UUID tokenID = UUID.Zero;
        public UUID userID = UUID.Zero;
        public bool canIssueChildTokens = false;
        public bool enabled;

        #region Query2Type members

        public new static List<VirtualTokenIssuer> doQuery2Type(List<string> query)
        {
            List<VirtualTokenIssuer> issuers = new List<VirtualTokenIssuer>(query.Count % 4 == 0 ? query.Count / 4 : 0);

            if (query.Count % 4 == 0)
            {
                for (int i = 0; i < query.Count; i += 4)
                {
                    uint foo;
                    bool canIssueChildTokens = uint.TryParse(query[i + 2], out foo) ? foo >= 1 : bool.Parse(query[i + 2]);
                    bool enabled = uint.TryParse(query[i + 3], out foo) ? foo >= 1 : bool.Parse(query[i + 3]);
                    issuers.Add(new VirtualTokenIssuer
                    {
                        tokenID = UUID.Parse(query[i]),
                        userID = UUID.Parse(query[i + 1]),
                        canIssueChildTokens = canIssueChildTokens,
                        enabled = enabled
                    });
                }
            }

            return issuers;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap(4);

            resp["tokenID"] = tokenID;
            resp["userID"] = userID;
            resp["canIssueChildTokens"] = canIssueChildTokens;
            resp["enabled"] = enabled;

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if (map.ContainsKey("tokenID"))
            {
                tokenID = UUID.Parse(map["tokenID"].ToString());
            }
            if (map.ContainsKey("userID"))
            {
                userID = UUID.Parse(map["userID"].ToString());
            }
            if (map.ContainsKey("canIssueChildTokens"))
            {
                canIssueChildTokens = bool.Parse(map["canIssueChildTokens"].ToString());
            }
            if (map.ContainsKey("enabled"))
            {
                enabled = bool.Parse(map["enabled"].ToString());
            }
        }

        public override bool Equals(object obj)
        {
            VirtualTokenIssuer isur = obj as VirtualTokenIssuer;
            return(
                isur.tokenID == tokenID &&
                isur.userID == userID &&
                isur.canIssueChildTokens == canIssueChildTokens &&
                isur.enabled == enabled
            );
        }

        #endregion

        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VirtualTokenCategory : Query2Type<VirtualTokenCategory>
    {
        public UUID id = UUID.Zero;
        public UUID parent = UUID.Zero;
        public string name = string.Empty;
        public string description = string.Empty;
        public DateTime created;

        #region Query2Type members

        public new static List<VirtualTokenCategory> doQuery2Type(List<string> query)
        {
            List<VirtualTokenCategory> categories = new List<VirtualTokenCategory>(query.Count % 5 == 0 ? query.Count / 5 : 0);

            if (query.Count % 5 == 0)
            {
                for (int i = 0; i < query.Count; i += 5)
                {
                    categories.Add(new VirtualTokenCategory
                    {
                        id = UUID.Parse(query[i]),
                        parent = UUID.Parse(query[i + 1]),
                        name = query[i + 2].Trim(),
                        description = query[i + 3].Trim(),
                        created = Utils.UnixTimeToDateTime(int.Parse(query[i + 4]))
                    });
                }
            }

            return categories;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap();

            resp["id"] = id;
            resp["parent"] = parent;
            resp["name"] = name;
            resp["description"] = description;
            resp["created"] = Utils.DateTimeToUnixTime(created);

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if (map.ContainsKey("id"))
            {
                id = UUID.Parse(map["id"].ToString());
            }
            if (map.ContainsKey("parent"))
            {
                parent = UUID.Parse(map["parent"].ToString());
            }
            if (map.ContainsKey("name"))
            {
                name = map["name"].ToString();
            }
            if (map.ContainsKey("description"))
            {
                description = map["description"].ToString();
            }
            if (map.ContainsKey("created"))
            {
                created = Utils.UnixTimeToDateTime(int.Parse(map["created"].ToString()));
            }
        }

        public override bool Equals(object obj)
        {
            VirtualTokenCategory cat = obj as VirtualTokenCategory;
            return (
                cat.id == id &&
                cat.parent == parent &&
                cat.name == name &&
                cat.description == description &&
                cat.created.CompareTo(created) == 0
            );
        }
   
        #endregion

        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum VirtualTokenTransactionType{
        Unknown = -1,
        System
    }

    public class VirtualTokenTransaction : Query2Type<VirtualTokenTransaction>{
        public UUID id = UUID.Zero;
        public UUID tokenID = UUID.Zero;
        public UUID sender = UUID.Zero;
        public UUID recipient = UUID.Zero;
        public DateTime issuedOn;
        public VirtualTokenTransactionType type;
        public int amount=0;
        public bool verified=false;
        private string m_message = string.Empty;
        public string message{
            get{
                return m_message;
            }
            set{
                m_message = value.Trim();
            }
        }

        #region Query2Type members
        
        public new static List<VirtualTokenTransaction> doQuery2Type(List<string> query){
            List<VirtualTokenTransaction> transactions = new List<VirtualTokenTransaction>(query.Count % 9 == 0 ? query.Count / 9 : 0);

            if(query.Count % 9 == 0){
                for(int i=0;i<query.Count; i += 9){
                    uint type = uint.Parse(query[i + 5]);
                    uint foo;
                    bool verified = uint.TryParse(query[i + 7], out foo) ? foo >= 1 : bool.Parse(query[i + 7]);
                    transactions.Add(new VirtualTokenTransaction{
                        id = UUID.Parse(query[i]),
                        tokenID = UUID.Parse(query[i + 1]),
                        sender = UUID.Parse(query[i + 2]),
                        recipient = UUID.Parse(query[i + 3]),
                        issuedOn = Utils.UnixTimeToDateTime(int.Parse(query[i + 4])),
                        type = (type <= (uint)Enum.GetValues(typeof(VirtualTokenTransactionType)).Cast<VirtualTokenTransactionType>().Max()) ? (VirtualTokenTransactionType)type : VirtualTokenTransactionType.Unknown,
                        amount = int.Parse(query[i + 6]),
                        verified = verified,
                        message = query[i + 8]
                    });
                }
            }

            return transactions;
        }

        #region IDataTransferable members

        public override OSDMap ToOSD()
        {
            OSDMap resp = new OSDMap(9);

            resp["id"] = id;
            resp["tokenID"] = tokenID;
            resp["sender"] = sender;
            resp["recipient"] = recipient;
            resp["issuedOn"] = Utils.DateTimeToUnixTime(issuedOn);
            resp["type"] = (int)type;
            resp["amount"] = amount;
            resp["verified"] = verified;
            resp["message"] = message;

            return resp;
        }

        public override void FromOSD(OSDMap map)
        {
            if(map.ContainsKey("id")){
                id = UUID.Parse(map["id"].ToString());
            }
            if(map.ContainsKey("tokenID")){
                tokenID = UUID.Parse(map["tokenID"].ToString());
            }
            if(map.ContainsKey("sender")){
                sender = UUID.Parse(map["sender"].ToString());
            }
            if(map.ContainsKey("recipient")){
                recipient = UUID.Parse(map["recipient"].ToString());
            }
            if(map.ContainsKey("issuedOn")){
                issuedOn = Utils.UnixTimeToDateTime(int.Parse(map["issuedOn"].ToString()));
            }
            if(map.ContainsKey("type")){
                uint maptype = uint.Parse(map["type"].ToString());
                type = (maptype <= (uint)Enum.GetValues(typeof(VirtualTokenTransactionType)).Cast<VirtualTokenTransactionType>().Max()) ? (VirtualTokenTransactionType)maptype : VirtualTokenTransactionType.Unknown;
            }
            if(map.ContainsKey("amount")){
                amount = int.Parse(map["amount"].ToString());
            }
            if(map.ContainsKey("verified")){
                verified = bool.Parse(map["verified"].ToString());
            }
            if(map.ContainsKey("message")){
                message = map["message"].ToString();
            }
        }

        public override bool Equals(object obj)
        {
            VirtualTokenTransaction foo = obj as VirtualTokenTransaction;

            return(
                foo.id == id &&
                foo.tokenID == tokenID &&
                foo.sender == sender &&
                foo.recipient == recipient &&
                foo.issuedOn.CompareTo(issuedOn) == 0 &&
                foo.type == type &&
                foo.amount == amount &&
                foo.verified == verified &&
                foo.message == message
            );
        }

        #endregion

        #endregion

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class VirtualTokenBalance
    {
        private VirtualToken m_Token;
        public VirtualToken Token
        {
            get { return m_Token; }
        }

        private int m_Balance;
        public int Balance
        {
            get { return m_Balance; }
        }

        private UUID m_User;
        public UUID User
        {
            get { return m_User; }
        }

        public VirtualTokenBalance(UUID user, VirtualToken token, int balance)
        {
            m_User = user;
            m_Token = token;
            m_Balance = balance;
        }
    }
}

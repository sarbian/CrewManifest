using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CrewManifest
{
    public class KerbalModel
    {
        public ProtoCrewMember Kerbal { get; set; }
        public bool IsNew { get; set; }
        public float Stupidity;
        public float Courage;
        public bool Badass;
        public string Name;
        public ProtoCrewMember.Gender Gender;
        public ProtoCrewMember.KerbalType Type;

        public KerbalModel(ProtoCrewMember kerbal, bool isNew)
        {
            this.Kerbal = kerbal;
            Name = kerbal.name;
            Stupidity = kerbal.stupidity;
            Courage = kerbal.courage;
            Badass = kerbal.isBadass;
            Gender = kerbal.gender;
            Type = kerbal.type;
            IsNew = isNew;
        }

        public string SubmitChanges()
        {
            if (NameExists())
            {
                return "That name is in use!";
            }
            
            if (IsNew)
            {
                Kerbal = HighLogic.CurrentGame.CrewRoster.GetNewKerbal();
                Kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            }

            SyncKerbal();

            return string.Empty;
        }

        public void SyncKerbal()
        {
            Kerbal.ChangeName(Name);
            Kerbal.stupidity = Stupidity;
            Kerbal.courage = Courage;
            Kerbal.isBadass = Badass;
            Kerbal.gender = Gender;
            Kerbal.type = Type;
        }

        private bool NameExists()
        {
            if(IsNew || Kerbal.name != Name)
            {
                return HighLogic.CurrentGame.CrewRoster[Name] != null;
            }

            return false;
        }
    }
}

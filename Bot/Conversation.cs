using Bot.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
    [System.Serializable]
    public class Conversation
    {

        stati? stato;
        CorsiEnum? corsienum;
        ScuoleEnums? scuolaenum;
        string percorso;

        public Conversation()
        {
            stato = stati.start;
        }
        public void setStato(stati? var)
        {
            stato = var;
        }
        public stati? getStato()
        {
            return stato;
        }

        internal void setCorso(CorsiEnum? corsienum)
        {
            this.corsienum = corsienum;
        }

        internal void setScuola(ScuoleEnums? scuolaenum)
        {
            this.scuolaenum = scuolaenum;
        }

        internal string getcorso()
        {
            return this.corsienum?.ToString();
        }

        internal void scesoDiUnLivello(string text)
        {
            if (string.IsNullOrEmpty(percorso))
            {
                percorso = text;
                return;
            }

            percorso += "/" + text;
        }

        internal string getPercorso()
        {
            return this.percorso;
        }
    }
}

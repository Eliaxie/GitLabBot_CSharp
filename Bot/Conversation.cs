using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
    public class Conversation
    {

        int stato;

        public Conversation()
        {
            stato = 1;
        }
        public int setStato(int newStato)
        {
            stato = newStato;
            return newStato;
        }
        public int getStato()
        {
            return stato;
        }
    }
}

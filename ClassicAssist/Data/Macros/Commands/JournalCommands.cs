﻿using System;
using System.Linq;
using System.Threading;
using Assistant;
using ClassicAssist.UO.Data;
using ClassicAssist.UO.Network;

namespace ClassicAssist.Data.Macros.Commands
{
    public static class JournalCommands
    {
        [CommandsDisplay( Category = "Journal", Description = "Check for a text in journal, optional source name." )]
        public static bool InJournal( string text, string author = "" )
        {
            bool match;

            if ( author.ToLower().Equals( "system" ) )
            {
                match = Engine.Journal.GetBuffer().Any( je =>
                    je.Text.ToLower().Contains( text.ToLower() ) && je.SpeechType == JournalSpeech.System );
            }
            else
            {
                match = Engine.Journal.GetBuffer()
                    .Any( je => je.Text.ToLower().Contains( text.ToLower() ) &&
                                ( string.IsNullOrEmpty( author ) || string.Equals( je.Name, author,
                                      StringComparison.CurrentCultureIgnoreCase ) ) );
            }

            return match;
        }

        [CommandsDisplay( Category = "Journal", Description = "Clear all journal texts." )]
        public static void ClearJournal()
        {
            Engine.Journal.Clear();
        }

        public static bool WaitForJournal( string text, int timeout, string author )
        {
            AutoResetEvent are = new AutoResetEvent( false );

            void OnIncomingPacketHandlersOnJournalEntryAddedEvent( JournalEntry je )
            {
                bool match;

                if ( author.ToLower().Equals( "system" ) )
                {
                    match = je.Text.ToLower().Contains( text.ToLower() ) && je.SpeechType == JournalSpeech.System;
                }
                else
                {
                    match = je.Text.ToLower().Contains( text.ToLower() ) &&
                            ( string.IsNullOrEmpty( author ) || string.Equals( je.Name, author,
                                  StringComparison.CurrentCultureIgnoreCase ) );
                }

                if ( match )
                {
                    are.Set();
                }
            }

            IncomingPacketHandlers.JournalEntryAddedEvent += OnIncomingPacketHandlersOnJournalEntryAddedEvent;

            bool result;

            try
            {
                result = are.WaitOne( timeout );
            }
            finally
            {
                IncomingPacketHandlers.JournalEntryAddedEvent -= OnIncomingPacketHandlersOnJournalEntryAddedEvent;
            }

            return result;
        }
    }
}
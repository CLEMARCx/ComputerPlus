﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rage.Forms;
using Gwen;
using Gwen.Control;
using Rage;
using LSPD_First_Response.Engine.Scripting.Entities;
using ComputerPlus.Extensions.Gwen;
namespace ComputerPlus.Interfaces.ComputerPedDB
{
    sealed class ComputerPedSearch : GwenForm
    {
        ListBox list_collected_ids;
        ListBox list_manual_results;

        TextBox text_manual_name;

        internal ComputerPedSearch() : base(typeof(ComputerPedSearchTemplate))
        {
        }

        public override void InitializeLayout()
        {
            base.InitializeLayout();
            text_manual_name.SetToolTipText("Name");
            this.Position = this.GetLaunchPosition();
            this.Window.DisableResizing();
            PopulateManualSearchPedsList();
            PopulateStoppedPedsList();
            list_manual_results.AllowMultiSelect = false;
            list_collected_ids.AllowMultiSelect = false;
            list_manual_results.RowSelected += onListItemSelected;
            list_collected_ids.RowSelected += onListItemSelected;
            text_manual_name.SubmitPressed += onSearchSubmit;
        }

        private void onSearchSubmit(Base sender, EventArgs arguments)
        {
            String name = text_manual_name.Text.ToLower();
            if (String.IsNullOrWhiteSpace(name)) return;
            ComputerPedController controller = ComputerPedController.Instance;
            var ped = controller.LookupPersona(name);
            if (ped != null)
            {
                if (!ped.Item1)
                {
                    text_manual_name.BoundsOutlineColor = System.Drawing.Color.Red;
                    text_manual_name.SetToolTipText("This person no longer exists");
                }
                else {
                    text_manual_name.ToolTip = null;
                    list_manual_results.AddPed(ped);
                    ComputerPedController.LastSelected = new Tuple<Ped, Persona>(ped.Item1, ped.Item2);
                    ComputerPedController.ActivatePedView();
                }
            } else
            {
                text_manual_name.BoundsOutlineColor = System.Drawing.Color.Red;
                text_manual_name.SetToolTipText("No persons found");
            }
        }

        private void AddPedPersonaToList(List<dynamic> list)
        {

        }

        public void PopulateManualSearchPedsList()
        {
            ComputerPedController controller = ComputerPedController.Instance;
            list_collected_ids.Clear();
            var results = controller.GetRecentSearches()
            .Where(x => x.Item1 != null && x.Item1.IsValid()).ToList(); 
            //@TODO choose if we want to remove null items from the list -- may cause user confusion
            if (results != null && results.Count > 0)
            {
                results.ForEach(x => list_manual_results.AddPed(x));
            }
        }

        public void PopulateStoppedPedsList()
        {
            ComputerPedController controller = ComputerPedController.Instance;
            list_collected_ids.Clear();
            var results = controller.PedsCurrentlyStoppedByPlayer.Where(x => x != null && x.IsValid()).ToArray();
            if (results != null && results.Length > 0)
                results
                .Select(x => controller.LookupPersona(x))
                .ToList()
                .ForEach(x => list_collected_ids.AddPed(x));
        }

        private void ClearSelections()
        {
            list_collected_ids.UnselectAll();
            list_manual_results.UnselectAll();
        }

        private void onListItemSelected(Base sender, ItemSelectedEventArgs arguments)
        {
            if (arguments.SelectedItem.UserData is Tuple<Ped, Persona>)
            {
                ComputerPedController.LastSelected = arguments.SelectedItem.UserData as Tuple<Ped, Persona>;                
                Game.LogVerboseDebug(String.Format("ComputerPedSearch.onListItemSelected updated ComputerPedController.LastSelected {0}", ComputerPedController.LastSelected.Item2.FullName));
                ClearSelections();
                ComputerPedController.ActivatePedView();
                Game.LogVerboseDebug("ComputerPedSearch.onListItemSelected successful");
            }
            else
            {
                Game.LogVerboseDebug("ComputerPedSearch.onListItemSelected arguments were not valid");
            }         
        }      
    }
}

﻿using RiotSharp;
using RiotSharp.Endpoints.ChampionMasteryEndpoint;
using RiotSharp.Endpoints.LeagueEndpoint;
using RiotSharp.Endpoints.MatchEndpoint;
using RiotSharp.Endpoints.StaticDataEndpoint.Champion;
using RiotSharp.Endpoints.StaticDataEndpoint.Item;
using RiotSharp.Endpoints.SummonerEndpoint;
using RiotSharp.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Terminal.Gui;
using RegMatch = System.Text.RegularExpressions.Match;

namespace LeagueTerminal.LeagueUtils
{
    class ItemsetGUI
    {
        static public View TopView;
        static public View TopViewWindow { get; set; }
        static private View SummonerSearchView { get; set; }

        static private View ChampionsView { get; set; }
        static private View ItemListWindow { get; set; }
        static private ListView ItemListView { get; set; }
        static private View ItemSetView { get; set; }
        static private RadioGroup BlockRadio { get; set; }
        static private ListView[] Blocks { get; set; }
        static private List<ItemListElement>[] BlockLists { get; set; }

        static private int SelectedBlockId { get; set; }

        static private TextField ItemName { get; set; }
        static private View ItemDetailsView { get; set; }

        static private RadioGroup TagsRadio { get; set; }


        static private Region SearchRegion { get; set; }

        //var latestVersion = allVersion[0]; // Example of version: "10.23.1"
        static private Dictionary<string, ChampionStatic> Champions { get; set; }

        static private Dictionary<int, ItemStatic> Items { get; set; }
        public static void SetupMainView(View myView)
        {

            Window itemsetWindow = new Window();
            Blocks = new ListView[4];
            BlockLists = new List<ItemListElement>[4];
            TopViewWindow = itemsetWindow;
            for (int i = 0; i < 4; i++)
            {
                BlockLists[i] = new List<ItemListElement>();
            }
            TopView = myView;
            SetupChampionSearch();
            SetupChampionList();
            SetupItemList();
            SetupItemDetails();
            SetupItemSetBlock();


            TopViewWindow.Add(SummonerSearchView);
            TopViewWindow.Add(ChampionsView);
            TopViewWindow.Add(ItemListWindow);
            TopViewWindow.Add(ItemDetailsView);
            TopViewWindow.Add(ItemSetView);

        }

        private static void SetSearchRegion(Region searchRegion)
        {
            SearchRegion = searchRegion;
        }

        private static void SetupChampionSearch()
        {
            var summonerSearchInfoWindow = new FrameView("Item search and filtering")
            {
                X = Pos.Percent(0),
                Y = 1, // Leave place for top level menu and Summoner search

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Percent(10)
            };

            ItemName = new TextField("")
            {
                X = Pos.Left(summonerSearchInfoWindow),
                Y = Pos.Percent(3),
                Width = 30,
                Height = 1
            };


            summonerSearchInfoWindow.Add(ItemName);

            NStack.ustring[] elo = new NStack.ustring[] { "Armor   ", "Damage   ", "SpellDamage   ", "AttackSpeed   " };
            RadioGroup test = new RadioGroup(elo)
            {
                X = Pos.Right(ItemName)+5,
                Y = Pos.Percent(3),
                Width = 40,
                Height = 1
            };
            test.DisplayMode = DisplayModeLayout.Horizontal;
            test.SelectedItemChanged += FilterItemList;
            TagsRadio = test;
            summonerSearchInfoWindow.Add(test);
            SummonerSearchView = summonerSearchInfoWindow;
        }

        private static void FilterItemList(RadioGroup.SelectedItemChangedArgs obj)
        {

            ItemListView.SetSource(LeagueUtilities.GetItemListByTag(TagsRadio.RadioLabels[obj.SelectedItem].ToString()));
        }

        private static void SetupChampionList()
        {
            var championListWindow = new FrameView("Champions")
            {
                X = Pos.Percent(0),
                Y = Pos.Percent(10) + 1, // Leave place for top level menu and Summoner search

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Percent(20),
                Height = Dim.Fill()
            };
            ChampionsView = championListWindow;


            var championsListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            championsListView.AllowsMultipleSelection = true ;
            championsListView.AllowsMarking = true;
            championsListView.SetSource(LeagueUtilities.GetChampionListElements());
            ChampionsView.Add(championsListView);

            //for (int i = 0; i < championsListView.Source.Count-1; i++)
            //{
            //    if (championsListView.Source.IsMarked(i))
            //    {
            //        championsListView.Source.ToList()[i]
            //    }
            //}
            

        }

        private static void SetupItemList()
        {
            var itemListWindow = new FrameView("Items")
            {
                X = Pos.Percent(20),
                Y = Pos.Percent(10) + 1, // Leave place for top level menu and Summoner search

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Percent(30),
                Height = Dim.Fill()
            };
            ItemListWindow = itemListWindow;


            var itemListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            itemListView.SetSource(LeagueUtilities.GetItemListElements());
            ItemListView = itemListView;
           
            ItemListWindow.Add(itemListView);
            itemListView.SelectedItemChanged += SetItemDetails;
            itemListView.OpenSelectedItem += ItemListView_OpenSelectedItem;
            //ItemView.ColorScheme = Colors.Dialog;
            //for (int i = 0; i < championsListView.Source.Count-1; i++)
            //{
            //    if (championsListView.Source.IsMarked(i))
            //    {
            //        championsListView.Source.ToList()[i]
            //    }
            //}


        }

        private static void ItemListView_OpenSelectedItem(ListViewItemEventArgs obj)
        {
            var listElement = (ItemListElement) obj.Value;
            var item = listElement.Item;

            BlockLists[SelectedBlockId].Add(listElement);
            Blocks[SelectedBlockId].SetSource(BlockLists[SelectedBlockId]);

        }

        private static void SearchButtonClickedHandler()
        {

          
        }

        private static void SetupItemDetails()
        {
            var itemInfoView = new FrameView("Item Details")
            {
                X = Pos.Percent(50),
                Y = Pos.Percent(10) + 1, // Leave place for top level menu and Summoner search

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = 12
            };

            ItemDetailsView = itemInfoView;
        }

        private static void SetupItemSetBlock()
        {
            var itemsetView = new FrameView("Itemset")
            {
                X = Pos.Percent(50),
                Y = Pos.Bottom(ItemDetailsView), // Leave place for top level menu and Summoner search

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            ItemSetView = itemsetView;

            NStack.ustring[] blocks = new NStack.ustring[] { "1   ", "2   ", "3   ", "4   " };
            var blockRadio = new RadioGroup(blocks)
            {
                X = Pos.Percent(40),
                Y = Pos.Percent(3),
                Width = Dim.Fill(),
                Height = 1
            };
            blockRadio.DisplayMode = DisplayModeLayout.Horizontal;
            ItemSetView.Add(blockRadio);

            var blockFrameView1 = new FrameView("Block1")
            {
                X = Pos.Percent(0),
                Y = Pos.Bottom(blockRadio),

                Width = Dim.Percent(50),
                Height = Dim.Percent(50)
            };
            var blockListView1 = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            Blocks[0] = blockListView1;
            blockFrameView1.Add(Blocks[0]);


            var blockFrameView2 = new FrameView("Block2")
            {
                X = Pos.Right(blockFrameView1),
                Y = Pos.Bottom(blockRadio),

                Width = Dim.Fill(),
                Height = Dim.Percent(50)
            };

            var blockListView2 = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            Blocks[1]= blockListView2;
            blockFrameView2.Add(Blocks[1]);


            var blockFrameView3 = new FrameView("Block3")
            {
                X = Pos.Percent(0),
                Y = Pos.Bottom(blockFrameView1),

                Width = Dim.Percent(50),
                Height = Dim.Fill()-1
            };
            var blockListView3 = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            Blocks[2] = blockListView3;
            blockFrameView3.Add(Blocks[2]);


            var blockFrameView4 = new FrameView("Block4")
            {
                X = Pos.Right(blockFrameView3),
                Y = Pos.Bottom(blockFrameView2),

                Width = Dim.Fill(),
                Height = Dim.Fill()-1
            };
            var blockListView4 = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };
            Blocks[3] = blockListView4;
            blockFrameView4.Add(Blocks[3]);


            ItemSetView.Add(blockFrameView1);
            ItemSetView.Add(blockFrameView2);
            ItemSetView.Add(blockFrameView3);
            ItemSetView.Add(blockFrameView4);
            
            SelectedBlockId = 0;
            Blocks[0].ColorScheme = Colors.Dialog;

            blockRadio.SelectedItemChanged += BlockRadio_SelectedItemChanged;

            Blocks[0].OpenSelectedItem += Block0openSelectedItem;
            Blocks[1].OpenSelectedItem += Block1openSelectedItem;
            Blocks[2].OpenSelectedItem += Block2openSelectedItem;
            Blocks[3].OpenSelectedItem += Block3openSelectedItem;

        }

        private static void Block0openSelectedItem(ListViewItemEventArgs obj)
        {
            BlockLists[0].Remove((ItemListElement) obj.Value);
            Blocks[0].SetSource(BlockLists[0]);
        }
        private static void Block1openSelectedItem(ListViewItemEventArgs obj)
        {
            BlockLists[1].Remove((ItemListElement)obj.Value);
            Blocks[1].SetSource(BlockLists[1]);
        }

        private static void Block2openSelectedItem(ListViewItemEventArgs obj)
        {
            BlockLists[2].Remove((ItemListElement)obj.Value);
            Blocks[2].SetSource(BlockLists[2]);
        }

        private static void Block3openSelectedItem(ListViewItemEventArgs obj)
        {
            BlockLists[3].Remove((ItemListElement)obj.Value);
            Blocks[3].SetSource(BlockLists[3]);
        }


        private static void BlockRadio_SelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
        {

            SelectedBlockId = obj.SelectedItem;
            
            for (int i = 0; i < 4; i++)
            {
                Blocks[i].ColorScheme = Colors.Base;
                if(i == obj.SelectedItem)
                    Blocks[SelectedBlockId].ColorScheme = Colors.Dialog;

            }
        }


        private static void SetItemDetails(ListViewItemEventArgs args)
        {
            ItemDetailsView.RemoveAll();

            var listElement = (ItemListElement)args.Value;
            var item = listElement.Item;

            var label = new Label("Item Name: " + item.Name)
            {
                X = Pos.Percent(0),
                Y = Pos.Percent(0),
                Width = Dim.Fill(),
                Height = 1
            };
            ItemDetailsView.Add(label);

            var costLabel = new Label("Cost: " + item.Gold.TotalPrice)
            {
                X = Pos.Percent(0),
                Y = Pos.Bottom(label),
                Width = Dim.Fill(),
                Height = 1
            };
            ItemDetailsView.Add(costLabel);

            var prev = costLabel;
            foreach (PropertyInfo prop in item.Stats.GetType().GetProperties())
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if ((double) prop.GetValue(item.Stats, null) != 0) {
                    var stat = new Label(string.Format("{0,-15}: {1,-15}", prop.Name, prop.GetValue(item.Stats, null)))
                    {
                        X = Pos.Percent(0),
                        Y = prev.Y + 1,
                        Width = Dim.Fill(),
                        Height = 1
                    };
                    prev = stat;
                    ItemDetailsView.Add(stat);
                }
            }
            string target = Regex.Replace(item.Description, "<[^>]*>", "");
            var description = new Label(target)
            {
                X = Pos.Percent(0),
                Y = prev.Y+1,
                Width = Dim.Fill(),
                Height = 7
            };
            ItemDetailsView.Add(description);



            //var emptylabel = new Label(" ")
            //{
            //    X = Pos.Percent(0),
            //    Y = Pos.Bottom(levelLabel),
            //    Width = Dim.Fill(),
            //    Height = 1
            //};
            //SummonerInfoView.Add(emptylabel);

            //var queue = new Label("Queue")
            //{
            //    X = Pos.Percent(0),
            //    Y = Pos.Bottom(emptylabel),
            //    Width = Dim.Fill(),
            //    Height = 1
            //};
            //SummonerInfoView.Add(queue);
        }

        private static void DrawInfoDialog(string message)
        {

            var ok = new Button("Ok");
            var entry = new TextField()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1
            };
            ok.Clicked += () => { Application.RequestStop(); };
            entry.Text = message;
            var dialog = new Dialog("Error", 60, 7, ok);
            dialog.Add(entry);
            Application.Run(dialog);
        }
    }
}
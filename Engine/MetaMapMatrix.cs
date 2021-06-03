using System;
using System.Collections.Generic;
using Game.Engine.Interactions;
using Game.Engine.Skills;
using System.IO;
using System.Text.RegularExpressions;
using Game.Engine.Interactions.InteractionFactories;

namespace Game.Engine
{
    [Serializable]
    class MetaMapMatrix
    {
        // world parameters
        private int randomInteractionMaps = 0; // number of maps that allows random interactions
        private int maps; // how many maps in total in the game world
        private int portals; // how many portals in each random map can't be greater than number of maps-1(but sometimes must be lower if maps have a lot of predefined portals)
        private int shops; // number of shops in the game world
        private int interactions; // approximate number of all interactions (including shops and quest encounters) in the game world (not strictly guaranteed due to quest constraints)
        private int monsters; // monsters per single map
        private int walls; // approximate number of walls per single map (not strictly guaranteed due to movement constraints)

        private GameSession parentSession;
        private List<Interaction> interactionList;
        private List<List<Interaction>> sideQuestInteractions;
        private Random rng = new Random();
        public List<Interaction> QuestElements { get; private set; } // shared between all maps
        private List<Interaction> mainQuest;
        // connections between maps
        private int[,] adjacencyMatrix; // a graph containing individual boards and connections between them
        private int[] visited;
        private int lastNumber;
        private int currentNumber;
        private int totalIntNumber;
        // maps
        private List<List<string>> fixedlist; //lista z odzielnymi słowami dla każdej mapy
        private MapMatrix[] matrix; // an array containing all boards in the game

        public MetaMapMatrix(GameSession parent)
        {
            parentSession = parent;
            fixedlist = new List<List<string>>();
            // read file
            var list = new List<string>();
            var rx = new Regex("\t"); //regex do tabulatur
            foreach (string el in File.ReadAllLines("../../Maps/MetaMap.txt")) // w folderze glownym gry nalezy stworzyc folder Maps i tam wrzucać pliki
            {
                list.Add(el);                       // otwieramy plik i spisujemy linijki
            }

            foreach (string el in list)
            {
                string[] tmp = rx.Split(el);
                var tmpList = new List<string>();
                foreach (string elm in tmp)
                {
                    tmpList.Add(elm);
                    if (tmpList.Contains("randominteractions=true")) { randomInteractionMaps++; } // zliczaj mapy pozwalające na losowe interakcje
                }
                fixedlist.Add(tmpList);
            }
            // read const
            string[] pair;
            maps = list.Count - 1;
            foreach (string el in fixedlist[0])
            {
                pair = el.Split('=');
                switch (pair[0])
                {
                    case "portals":
                        portals = Convert.ToInt32(pair[1]);
                        break;
                    case "shops":
                        shops = Convert.ToInt32(pair[1]);
                        break;
                    case "interactions":
                        interactions = Convert.ToInt32(pair[1]);
                        break;
                    case "monsters":
                        monsters = Convert.ToInt32(pair[1]);
                        break;
                    case "walls":
                        walls = Convert.ToInt32(pair[1]);
                        break;
                }
            }
            // create adjacency matrix
            CreateAdjencymatrix();
            // generate interactions
            GenerateInteractions();
            GenerateQuests();
            CheckPregeneratedQuests();
            totalIntNumber = interactionList.Count;
            // create maps
            matrix = new MapMatrix[maps];
            for (int i = 0; i < maps; i++)
            {
                if (fixedlist[i + 1][0].Contains("file="))
                {
                    File.WriteAllText("../../Maps/Debug.txt", "map from \"file=\" was created");
                    matrix[i] = new MapMatrix(parentSession, "../../Maps/" + fixedlist[i + 1][0].Split('=')[1]);
                }
                else
                {
                    matrix[i] = new MapMatrix(parentSession, MakePortalsList(i), CreateQuests(i), rng.Next(1000 * maps), (monsters, walls));
                    //matrix[i] = new MapMatrix(parentSession, MakePortalsList(i), CreateQuests(i), rng.Next(1000 * maps), CreateMonsters(i), walls);
                }
            }
        }
        private void CreateAdjencymatrix()
        {
            adjacencyMatrix = new int[maps, maps];
            for (int j = 0; j < maps; j++)
            {
                foreach (string el in fixedlist[j + 1]) // for each map check if file contains portals
                {
                    if (el.Contains("portal"))
                    {
                        if (el.Equals("randomportals=true"))
                        {
                            int counter = 0;
                            for (int b = 0; b < portals; b++)
                            {
                                if (counter == maps * maps)
                                {
                                    break;
                                }
                                counter++;
                                int a = rng.Next(maps);
                                if (fixedlist[a + 1].Contains("randomportals=true"))
                                {
                                    if (a == j || adjacencyMatrix[a, j] == 1) { b--; continue; }
                                    adjacencyMatrix[a, j] = 1;
                                    adjacencyMatrix[j, a] = 1;
                                }
                                else
                                {
                                    b--;
                                    continue;
                                }
                            }
                        }
                        if (!el.Contains("randomportals"))
                        {
                            int portalNumber = Convert.ToInt32((el.Split('=')[1]));
                            adjacencyMatrix[portalNumber, j] = 1;
                            adjacencyMatrix[j, portalNumber] = 1;
                        }
                    }
                }
            }
            if (!CheckConnectivity())
            {
                File.WriteAllText("../../Maps/Error.txt", "Couldn't find possible connection between maps");
            }
        }
        private bool CheckConnectivity()
        {
            // utility: check if the adjacencyMatrix represents a fully connected graph
            visited = new int[maps];
            SearchAndMark(0);
            for (int i = 0; i < maps; i++)
            {
                if (visited[i] == 0) return false;
            }
            return true;
        }
        private void SearchAndMark(int nodeNumber)
        {
            // utility for the CheckConnectivity method
            visited[nodeNumber] = 1;
            for (int i = 0; i < maps; i++)
            {
                if (visited[i] == 1) continue;
                if (adjacencyMatrix[i, nodeNumber] == 0) continue;
                SearchAndMark(i);
            }
        }
        private void GenerateInteractions()
        {
            // fill the game world with interactions
            interactionList = new List<Interaction>();
            for (int i = 0; i < shops; i++) interactionList.Add(new ShopInteraction(parentSession));
            for (int i = shops; i < interactions; i++)
            {
                List<Interaction> tmp = Index.DrawInteractions(parentSession);
                if (tmp != null) interactionList.AddRange(tmp);
            }
        }
        private void GenerateQuests()
        {
            sideQuestInteractions = new List<List<Interaction>>();
            QuestElements = Index.MainQuestFactory.CreateInteractionsGroup(parentSession);
            mainQuest = QuestElements;
            foreach (InteractionFactory el in Index.SideQuestFactory)
            {
                sideQuestInteractions.Add(el.CreateInteractionsGroup(parentSession));
            }
        }
        private void CheckPregeneratedQuests()
        {
            //check all stages of main quest
            List<int> put = new List<int>();
            for (int k = 0; k < mainQuest.Count; k++)
            {
                put.Add(k);
            }
            for (int i = 0; i < maps; i++)
            {
                foreach (string el in fixedlist[i + 1])
                {
                    if (el.Contains("mainquest="))
                    {
                        int questNumber = Convert.ToInt32(el.Split('=')[1]) - 1;
                        put.Remove(questNumber);
                    }
                }
            }
            foreach (int el in put)
            {
                interactionList.Add(mainQuest[el]);
            }
            //now side quests they won't always be in game if they are not predefined
            List<List<int>> putSide = new List<List<int>>();
            List<string> pregeneratedQuestNumbers = new List<string>();
            for (int i = 0; i < sideQuestInteractions.Count; i++)
            {
                putSide.Add(new List<int>());
                for (int j = 0; j < sideQuestInteractions[i].Count; j++)
                {
                    putSide[i].Add(j);
                }
            }
            for (int i = 0; i < maps; i++)
            {
                foreach (string el in fixedlist[i + 1])
                {
                    if (el.Contains("sidequest="))
                    {
                        string questNumber = el.Split('=')[1];
                        string[] questPoint = questNumber.Split('.');
                        if (!pregeneratedQuestNumbers.Contains(questPoint[0]))
                        {
                            pregeneratedQuestNumbers.Add(questPoint[0]);
                        }
                        putSide[Convert.ToInt32(questPoint[0]) - 1].Remove(Convert.ToInt32(questPoint[1]) - 1);
                    }
                }
            }
            foreach (string el in pregeneratedQuestNumbers)
            {
                foreach (int elm in putSide[Convert.ToInt32(el) - 1])
                {
                    interactionList.Add(sideQuestInteractions[Convert.ToInt32(el) - 1][elm]);
                }
            }
        }
        private List<int> MakePortalsList(int node)
        {
            // utility method for converting from matrix to list of portals
            List<int> ans = new List<int>();
            for (int i = 0; i < maps; i++)
            {
                if (adjacencyMatrix[i, node] == 1) ans.Add(i);
            }
            return ans;
        }
        private List<Interaction> CreateQuests(int i)
        {
            List<Interaction> tmp = new List<Interaction>();
            foreach (string el in fixedlist[i + 1])
            {
                if (el.Contains("sidequest="))
                {
                    string questNumber = el.Split('=')[1];
                    string[] questPoints = questNumber.Split('.');
                    tmp.Add(sideQuestInteractions[Convert.ToInt32(questPoints[0]) - 1][Convert.ToInt32(questPoints[1]) - 1]);
                }
                if (el.Contains("mainquest="))
                {
                    int questPoint = Convert.ToInt32(el.Split('=')[1]) - 1;
                    tmp.Add(mainQuest[questPoint]);
                }
            }
            return CreateInteractions(tmp, i);
        }
        private List<Interaction> CreateInteractions(List<Interaction> _tmp, int i)
        {
            List<Interaction> tmp = _tmp;
            foreach (string el in fixedlist[i + 1]) //map contains predefined interactions
            {
                if (el.Contains("interactionFactory="))
                {
                    string interactionNumber = el.Split('=')[1];
                    tmp.AddRange(Index.InteractionFactories[Convert.ToInt32(interactionNumber) - 1].CreateInteractionsGroup(parentSession));
                }
                if (el.Contains("interaction="))
                {
                    string interactionNumber = el.Split('=')[1];
                    tmp.Add(Index.GetInteractionByNumber(parentSession, Convert.ToInt32(interactionNumber)));
                }
            }
            if (fixedlist[i + 1].Contains("randominteractions=true")) //map contains random interactions
            {
                if (i == maps - 1) tmp.AddRange(interactionList);
                else
                {
                    for (int j = 0; j < totalIntNumber / randomInteractionMaps; j++)
                    {
                        if (interactionList.Count == 0) break;
                        int index = rng.Next(interactionList.Count);
                        tmp.Add(interactionList[index]);
                        interactionList.RemoveAt(index);
                    }
                }
            }
            return tmp;
        }
        // public
        public MapMatrix GetCurrentMatrix(int codeNumber)
        {
            // get the currently used board
            lastNumber = currentNumber;
            currentNumber = codeNumber;
            return matrix[codeNumber];
        }
        public int GetPreviousMatrixCode()
        {
            // for display when portal hopping
            // each board has its own individual code, which is used by portals to remember which portal leads where
            // example: let's say we have a board with code 34
            // this means that portals leading to that board will be encoded as 2034 value everywhere in the game
            return lastNumber;
        }
        // in-game map modifications
        public void AddMonsterToRandomMap(Monsters.MonsterFactories.MonsterFactory factory)
        {
            int mapNumber = currentNumber;
            while (mapNumber == currentNumber) mapNumber = Index.RNG(0, maps); // random map other than the current one
            while (true)
            {
                int x = Index.RNG(2, matrix[mapNumber].Width - 2);
                int y = Index.RNG(2, matrix[mapNumber].Height - 2);
                if (!matrix[mapNumber].ValidPlace(x, y))
                {
                    continue;
                }
                matrix[mapNumber].Matrix[y, x] = 1000;
                matrix[mapNumber].MonDict.Add(matrix[mapNumber].Width * y + x, factory);
                break;
            }
        }
        public void AddInteractionToRandomMap(Interaction interaction)
        {
            int mapNumber = currentNumber;
            while (mapNumber == currentNumber) mapNumber = Index.RNG(0, maps); // random map other than the current one
            while (true)
            {
                int x = Index.RNG(2, matrix[mapNumber].Width - 2);
                int y = Index.RNG(2, matrix[mapNumber].Height - 2);
                if (!matrix[mapNumber].ValidPlace(x, y))
                {
                    continue;
                }
                if (matrix[mapNumber].Interactions.ContainsKey(matrix[mapNumber].Width * y + x)) continue;
                matrix[mapNumber].Interactions.Add(matrix[mapNumber].Width * y + x, interaction);
                matrix[mapNumber].Matrix[y, x] = 3000 + Int32.Parse(interaction.Name.Replace("interaction", ""));
                break;
            }
        }

    }
}

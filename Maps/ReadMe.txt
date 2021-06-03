W folderze bin/debug/maps znajduje się plik o nazwie "MetaMap.txt".
Plik ten pozwala na tworzenie map z włąsnymi połaczeniami, potworami i interakcjami.
Nazwy tego pliku nie należy zmieniać pod żadnym pozorem.
Każda linijka poza pierwszą w tym pliku odpowiada kolejnej mapie, któa pojawi się w grze.

Pierwsza linijka pozwala na modyfikowanie globalnych ustawień:
portals=	// oczekiwana licza portali na każdej losowej mapie
shops=	// ilość sklepów w grze, które pojawią się losowo na mapie
interactions=	// ilość losowych interakcji (włączając w to sklepy) w grze pojawiających się na mapach, które na to pozwalają. Ta liczba często będzie się różnić przez interakce questowe
monsters=	// ilość potworów na każdej mapie, która pozwala na losowe potwory.
walls=	// oczekiwana iczba ścian na mapie(ta lczba możę się różnić, by pozwolić na dotarcie w każde pole mapy)

W każdej linijce dostępne są następujące flagi:
randomportals=true/false	// Na tej mapie będą tworzyć się losowe portale między innymi mapami, któe na to pozwalają
randominteractions=true/false	// Na tej mapie będą pojawiać się losowe interakcje i wydarzenia questowe, które nie zostały ujęte w innych mapach
randommonsters=true/false	// Na tej mapie będą pojawiać się loswe potwory
portal=(liczba)	// Na tej mapie zostanie stworzony portal do odpowiedniej mapy. Mapy numerowane są od zera, rozpoczynając od drugiej linijki pliku. (Tzn. mapa z linijki n ma numer w grze n-2).
monster=(liczba)	// Na tej mapie pojawi się potwór z fabryki o numerze (liczba) w liście Index.monsterFactories


mainquest=(liczba)	// Na tej mapie pojawi się interakcja głównego questa. Liczba mówi, która interakcja ma się pojawić. Interakcje, które nie zostaną ręcznie ustalone pojawią się na mapach pozwalających na losowe interakcje.
sidequest=(liczba).(liczba)	// Na tej mapie pojawi się interakcja questa pobocznego. Pierwsza liczba oznacza, z którego questa pochodzi interakcja(liczba ta odpowiada kolejnym interakcjom zawartym w Index.SideQuestFactory). Druga liczba mówi, która interakcja ma zostać położona na mapie. Reszta interakcji pojawi się tak samo jak w przypadku mainquest. Jeżeli żadna z interakcji nie zostanie ręcznie umeiszczona, ten sidequest nie pojawi się w  grze.
interactionFactory = (liczba)	// Na tej mapie pojawi się interakcja z fabryki o  numerze (liczba) w liście Index.InteractionFactories
interaction = (liczba)	// Na tej mapie pojawi się interakcja o nazwie "interaction(liczba)"

file=(ścieżka). Dzieki tej fladze można stworzyć mapę opisując każde pole numerem(patrz MapMatrix).
Musi to być plik typu .txt zawierający w sobie numery odpoiwadające polom mapy odzielone w tej samej linijce tabulatorami, a linijki odzielone enterami.
Rozmiar mapy to 24x18.

UWAGA 1!
Jeżeli korzysta się z flagi file=, nalezy rónież w tej linijce umieścić flagi mówiące o tym jakie portale i questy znajdują się na mapie.
UWAGA 2!
Flagi w tej samej linijce muszą być odzielone tabulatorami!
UWAGA 3!
Jeżeli przynajmniej jedna mapa posiada flagę randominteractions=true, to jedna z tych map musi znajdować się w pliku MetaMapMatrix jako ostatnia.
UWAGA 4!
W linijkach opisujacych mapy można również pisać kometarze oddzielone tabulatorami, jednak nie mogą one zawierać w sobie nazwy flagi ani znaczącej jej części.

Przykłady: pliki MetaMapMatrix.txt oraz MyMap.txt

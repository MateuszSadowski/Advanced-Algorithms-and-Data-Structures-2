using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = false;
        
        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo,
            out SimpleWeeklyPlan[] weeklyPlan)
        {
            (Graph, Graph) networks = BuildNetworks(production, sales, storageInfo);

            int weeksCount = production.Length;
            int n = weeksCount + 2;
            int source = n - 2;
            int sink = n - 1;

            var result = MinCostFlowGraphExtender.MinCostFlow(networks.Item1, networks.Item2, source, sink);    //TODO: might have to add delegates

            weeklyPlan = BuildWeeklyPlans(result.flow);
            return new PlanData {Quantity = (int)result.value, Value = -result.cost};
        }

        internal (Graph, Graph) BuildNetworks(PlanData[] production, PlanData[] sales, PlanData storageInfo)
        {
            int weeksCount = production.Length;
            var quantityNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, weeksCount + 2);
            var costNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, weeksCount + 2);

            int n = quantityNetwork.VerticesCount;
            int source = n - 2; 
            int sink = n - 1;   
            for (int week = 0; week < weeksCount; week++)
            {
                //production
                quantityNetwork.AddEdge(source, week, production[week].Quantity);
                costNetwork.AddEdge(source, week, production[week].Value);

                //sales
                quantityNetwork.AddEdge(week, sink, sales[week].Quantity);
                costNetwork.AddEdge(week, sink, -sales[week].Value);  //cost < 0 => profit

                //storage
                if(week < weeksCount - 1)
                {
                    quantityNetwork.AddEdge(week, week + 1, storageInfo.Quantity);
                    costNetwork.AddEdge(week, week + 1, storageInfo.Value);
                }
            }

            return (quantityNetwork, costNetwork);
        }

        internal SimpleWeeklyPlan[] BuildWeeklyPlans(Graph flow)
        {
            //var ge = new GraphExport();
            //ge.Export(flow);
            int n = flow.VerticesCount;
            int weeksCount = n - 2;
            var plans = new SimpleWeeklyPlan[weeksCount];

            int source = n - 2;
            int sink = n - 1;

            for (int v = 0; v < n - 1; v++)
            {
                foreach (var e in flow.OutEdges(v))
                {
                    //production
                    if(e.From == source)
                    {
                        int week = e.To;
                        plans[week].UnitsProduced = (int)e.Weight;
                    }

                    //sales
                    else if (e.To == sink)
                    {
                        int week = e.From;
                        plans[week].UnitsSold = (int)e.Weight;
                    }

                    //storage
                    else if (e.From != source && e.To != sink)
                    {
                        int week = e.From;
                        plans[week].UnitsStored = (int)e.Weight;
                    }
                }
            }

            return plans;
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
            out WeeklyPlan[] weeklyPlan)
        {
            weeklyPlan = null;
            return new PlanData {Quantity = -1, Value = 0};
        }

    }
}
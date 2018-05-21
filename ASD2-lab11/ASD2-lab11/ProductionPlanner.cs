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

            var result = MinCostFlowGraphExtender.MinCostFlow(networks.Item1, networks.Item2, source, sink, true, MaxFlowGraphExtender.PushRelabelMaxFlow, MaxFlowGraphExtender.OriginalDinicBlockingFlow, true);

            weeklyPlan = BuildWeeklyPlans(result.flow);
            return new PlanData {Quantity = (int)result.value, Value = -result.cost};
        }

        internal (Graph, Graph) BuildNetworks(PlanData[] production, PlanData[] sales, PlanData storageInfo)
        {
            int weeksCount = production.Length;
            int n = weeksCount + 2;
            var quantityNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n);
            var costNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n);

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
            (Graph, Graph) networks = BuildNetworksMaxProfitOnly(production, sales, storageInfo);

            int weeksCount = production.Length;
            int salesmenCount = sales.GetLength(0);
            int n = networks.Item1.VerticesCount;
            int source = n - 3;
            int sink = n - 1;

            var result = MinCostFlowGraphExtender.MinCostFlow(networks.Item1, networks.Item2, source, sink, true, MaxFlowGraphExtender.PushRelabelMaxFlow, MaxFlowGraphExtender.OriginalDinicBlockingFlow, true);
            double producedCount;
            WeeklyPlan[] plans = BuildWeeklyPlansMaxProfitOnly(result.flow, weeksCount, salesmenCount, out producedCount);

            weeklyPlan = plans;
            return new PlanData {Quantity = (int)producedCount, Value = -result.cost};
        }

        internal WeeklyPlan[] BuildWeeklyPlansMaxProfitOnly(Graph flow, int weeksCount, int salesmenCount, out double producedCount)
        {
            int n = flow.VerticesCount;
            var plans = new WeeklyPlan[weeksCount];
            double producedUnits = 0;

            int factory = n - 2;
            int sink = n - 1;

            for (int week = 0; week < weeksCount; week++)
            {
                plans[week].UnitsSold = new int[salesmenCount];
            }

            for (int v = 0; v < n - 1; v++)
            {
                foreach (var e in flow.OutEdges(v))
                {
                    //production
                    if (e.From == factory)
                    {
                        if (e.To == sink)
                            continue;

                        int week = e.To;
                        plans[week].UnitsProduced = (int)e.Weight;
                        producedUnits += (int)e.Weight;
                    }

                    //sales
                    else if (e.From < weeksCount && e.To >= weeksCount && e.To < weeksCount + salesmenCount)
                    {
                        int week = e.From;
                        int salesmen = e.To - weeksCount;
                        plans[week].UnitsSold[salesmen] = (int)e.Weight;
                    }

                    //storage
                    else if (e.From < weeksCount && e.To < weeksCount)
                    {
                        int week = e.From;
                        plans[week].UnitsStored = (int)e.Weight;
                    }
                }
            }

            producedCount = producedUnits;
            return plans;
        }

        internal (Graph, Graph) BuildNetworksMaxProfitOnly(PlanData[] production, PlanData[,] sales, PlanData storageInfo)
        {
            int weeksCount = production.Length;
            int salesmenCount = sales.GetLength(0);
            int n = salesmenCount + weeksCount + 2 + 1;
            var quantityNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n);
            var costNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n);

            int source = n - 3;
            int factory = n - 2;
            int sink = n - 1;
            int maxProduction = 0;

            for (int week = 0; week < weeksCount; week++)
            {
                //production
                quantityNetwork.AddEdge(factory, week, production[week].Quantity);
                costNetwork.AddEdge(factory, week, production[week].Value);
                maxProduction += production[week].Quantity;

                for (int salesman = weeksCount; salesman < weeksCount + salesmenCount; salesman++)
                {
                    //from factory to salesman
                    quantityNetwork.AddEdge(week, salesman, sales[salesman - weeksCount, week].Quantity);
                    costNetwork.AddEdge(week, salesman, -sales[salesman - weeksCount, week].Value);
                }

                //storage
                if (week < weeksCount - 1)
                {
                    quantityNetwork.AddEdge(week, week + 1, storageInfo.Quantity);
                    costNetwork.AddEdge(week, week + 1, storageInfo.Value);
                }
            }

            for (int salesman = weeksCount; salesman < weeksCount + salesmenCount; salesman++)
            {
                //from salesman to sink
                quantityNetwork.AddEdge(salesman, sink, Int32.MaxValue);
                costNetwork.AddEdge(salesman, sink, 0);
            }

            //source to factory (all weeks together)
            quantityNetwork.AddEdge(source, factory, maxProduction);
            costNetwork.AddEdge(source, factory, 0);

            //extra edge filtering unprofitable production
            quantityNetwork.AddEdge(factory, sink, Int32.MaxValue);
            costNetwork.AddEdge(factory, sink, 0);

            return (quantityNetwork, costNetwork);
        }
    }
}
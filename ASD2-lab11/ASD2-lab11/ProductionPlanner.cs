﻿using System;
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
            (Graph, Graph) networks = BuildNetworksMaxProfitOnly(production, sales, storageInfo);

            int weeksCount = production.Length;
            int n = networks.Item1.VerticesCount;
            int source = n - 3;
            int sink = n - 1;

            var result = MinCostFlowGraphExtender.MinCostFlow(networks.Item1, networks.Item2, source, sink);
            //var ge = new GraphExport();
            //ge.Export(result.flow);
            //ge.Export(networks.Item1);
            double producedCount;
            BuildWeeklyPlansMaxProfitOnly(result.flow, out producedCount);


            weeklyPlan = null;
            return new PlanData {Quantity = (int)producedCount, Value = -result.cost};
        }

        internal SimpleWeeklyPlan[] BuildWeeklyPlansMaxProfitOnly(Graph flow, out double producedCount)
        {
            //var ge = new GraphExport();
            //ge.Export(flow);
            int n = flow.VerticesCount;
            double produced = 0;
            foreach (var e in flow.OutEdges(n - 2))
            {
                if (e.To == n - 1)
                    continue;
                produced += e.Weight;
            }


            producedCount = produced;
            return null;
        }

        internal (Graph, Graph) BuildNetworksMaxProfitOnly(PlanData[] production, PlanData[,] sales, PlanData storageInfo)
        {
            int weeksCount = production.Length;
            int salesmansCount = sales.GetLength(0);
            int n = weeksCount * salesmansCount + weeksCount + 2 + 1;
            var quantityNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n);
            var costNetwork = new AdjacencyListsGraph<SimpleAdjacencyList>(true, n);

            int source = n - 3;
            int factory = n - 2;
            int sink = n - 1;
            //0 -> weeksCount - 1 => production per week
            //var extraVertices = new int[weeksCount];
            //for (int week = 0; week < weeksCount; week++)
            //    extraVertices[week] = weeksCount + week;
            var salesmanInWeek = new int[weeksCount, salesmansCount];
            for (int week = 0; week < weeksCount; week++)
                for (int salesman = 0; salesman < salesmansCount; salesman++)
                    salesmanInWeek[week, salesman] = weeksCount + week * salesmansCount + salesman;

            int maxSaleValue = 0;
            int maxProduction = 0;

            for (int week = 0; week < weeksCount; week++)
            {
                //production
                quantityNetwork.AddEdge(factory, week, production[week].Quantity);
                costNetwork.AddEdge(factory, week, production[week].Value);
                maxProduction += production[week].Quantity;

                for (int salesman = 0; salesman < salesmansCount; salesman++)
                {
                    //from factory to salesman
                    quantityNetwork.AddEdge(week, salesmanInWeek[week, salesman], Int32.MaxValue);
                    costNetwork.AddEdge(week, salesmanInWeek[week, salesman], 0);

                    //from salesman to sink
                    quantityNetwork.AddEdge(salesmanInWeek[week, salesman], sink, sales[salesman, week].Quantity);
                    costNetwork.AddEdge(salesmanInWeek[week, salesman], sink, -sales[salesman, week].Value);
                    maxSaleValue += sales[salesman, week].Quantity;
                }

                //quantityNetwork.AddEdge(week, sink, production[week].Quantity - maxSaleValue < 0 ? 0 : production[week].Quantity - maxSaleValue);
                //costNetwork.AddEdge(week, sink, 0);
                //quantityNetwork.AddEdge(extraVertices[week], sink, Int32.MaxValue);
                //costNetwork.AddEdge(extraVertices[week], sink, 0);

                //storage
                if (week < weeksCount - 1)
                {
                    quantityNetwork.AddEdge(week, week + 1, storageInfo.Quantity);
                    costNetwork.AddEdge(week, week + 1, storageInfo.Value);
                }
            }

            quantityNetwork.AddEdge(source, factory, maxProduction);
            costNetwork.AddEdge(source, factory, 0);

            quantityNetwork.AddEdge(factory, sink, Int32.MaxValue);
            costNetwork.AddEdge(factory, sink, 0);

            //var ge = new GraphExport();
            //ge.Export(quantityNetwork);
            //ge.Export(costNetwork);
            return (quantityNetwork, costNetwork);
        }
    }
}
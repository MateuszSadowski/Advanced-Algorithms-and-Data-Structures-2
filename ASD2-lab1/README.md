
Napisa� 3 implementacje kolejki priorytetowej

1) LazyPriorityQueue - "leniwa" implementacja (zwyk�a lista)
- operacja wstawiania ma z�o�ono�� sta��
- operacja pobierania ma z�o�ono�� liniow�

2) EagerPriorityQueue - "gorliwa" implementacja (lista posortowana)
- operacja wstawiania ma z�o�ono�� liniow�
- operacja pobierania ma z�o�ono�� sta��

3) HeapPriorityQueue - implementacja za pomoc� kopca binarnego
- operacja wstawiania ma z�o�ono�� logarytmiczn�
- operacja pobierania ma z�o�ono�� logarytmiczn�

Ka�da z implementacji w przypadku pr�by pobrania/pokazania maksymalnego elementu z pustej kolejki
(metody GetMax i ShowMax) powinna zg�asza� wyj�tek InvalidOperationException (to klasa standardowa)
z komunikatem "Access to empty queue".

Mo�na korzysta� ze standardowych kolekcji C#, ale z zachowaniem wymaganych w zadaniu z�o�ono�ci.
Nie mo�na za�o�y�, �e ka�da pojedyncza operacja na kolekcji (wywo�anie metody) ma z�o�ono�� sta��.
Trzeba umie� wyt�umaczy� z�o�ono�� obliczeniow� rozwi�zania.

Punktacja:
- LazyPriorityQueue  -  1p
- EagerPriorityQueue -  1p
- HeapPriorityQueue  -  2p

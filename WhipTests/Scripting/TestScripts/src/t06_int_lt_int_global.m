#include test.mi

Global int A = 42;
Global int B = 100500;

System.OnMain()
{
	System.IsTrue(A < B);
}
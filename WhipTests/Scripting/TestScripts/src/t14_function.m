#include test.mi

extern class @{340E4D7D-B394-4B58-90EB-14523F93D3C5}@ Object &TestObject;

function test();

System.OnMain()
{
	test();
}

test()
{
	System.IsTrue(1);
}
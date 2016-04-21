#include test.mi

extern Object System.getScriptGroup();

System.OnMain()
{
	System.IsTrue(System.getScriptGroup() != NULL);
}
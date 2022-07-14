# LSystem
Library for working with [L-systems](https://en.wikipedia.org/wiki/L-system). You can read more [here](http://algorithmicbotany.org/papers/#abop).

```C#
public static void Main(string[] args)
{
    var A = new UniversalModule("A"); // first module A
    var B = new UniversalModule("B"); // second module B
    var rule1 = new UniversalRule(A, new []{A, B}); // rule A -> AB
    var rule2 = new UniversalRule(B, new []{A}); //rule B -> A
    var lSys = new BasicLSystem<UniversalModule, UniversalRule>(new []{B}, new []{A, B}, new []{rule1, rule2}); // l-system creation
    lSys.Init();
    Display(lSys.CurrentState); // display axiome
    for (var i = 0; i < 5; i++)
    {
        // make substitution and display
        lSys.Step();
        Display(lSys.CurrentState);
    }
}

private static void Display(IEnumerable<IModule> state)
{
    foreach (var m in state)
    {
        Console.Write($"{m.Id}; ");
    }
    Console.WriteLine();
}

//B;
//A;
//A; B;
//A; B; A;
//A; B; A; A; B;
//A; B; A; A; B; A; B; A;
```

supports any kind of parameters

```C#
public static void Main(string[] args)
{
    var numberParameter = new BasicParameter<int>(1, "num"); // declare int parameter "num" with initial value 0
    var A = new UniversalModule("A", new[] { numberParameter }); // assign parameter "num" to module A
    var B = new UniversalModule("B");

    //create parameter transformation. It is used to change parameters in substitution. This particular one changes only one parameter
    var parameterTransformation = new ParameterTransformation<int>("num", (v) => v + 1); // it finds int value of parameter "num" of predecessor and assigns 
    // value + 1 to parameter "num" of first successor;

    var rule1 = new UniversalRule(A, new []{A, B}, new[] { parameterTransformation }); // assign parameter transformation to substitution rule
    var rule2 = new UniversalRule(B, new []{A}); // this rule does not have any parameter transformations but A has a parameter "num". It's value 
    // will be set to 1 as it is initial value of "num" parameter.
    var lSys = new BasicLSystem<UniversalModule, UniversalRule>(new []{B}, new []{A, B}, new []{rule1, rule2});
    lSys.Init();
    Display(lSys.CurrentState);
    for (var i = 0; i < 5; i++)
    {
        lSys.Step();
        Display(lSys.CurrentState);
    }
}

private static void Display(IEnumerable<IModule> state)
{
    foreach (var m in state)
    {
        if (m.parameters.Count() == 0)
            Console.Write($"{m.Id}; ");
        else
        {
            Console.Write($"{m.Id}({string.Join(", ", m.parameters)}); ");
        }
    }
    Console.WriteLine();
}

//B;
//A(num=1);
//A(num=2); B;
//A(num=3); B; A(num=1);
//A(num=4); B; A(num=1); A(num=2); B;
//A(num=5); B; A(num=1); A(num=2); B; A(num=3); B; A(num=1);
```

Alsough lib allows defining L-system with xml document. For example following file (for example named `l-system.xml`) defines L-system presented above

```xml
<?xml version="1.0"?>
<lsml xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
	xsi:schemaLocation="LSystem https://raw.githubusercontent.com/hevezolly/LSystem/master/XmlLSystem/LSML.xsd"
	xmlns="LSystem">

	<declaration>

		<parameters>
			<parameter type="int" param_id="num">1</parameter>
		</parameters>

		<modules>
			<module id="A">
				<parameter param_id="num"/>
			</module>
			<module id="B"></module>
		</modules>

		<axiome>
			<module id="B"/>
		</axiome>

	</declaration>

	<rule source="A">
		<successor id="A">
			<parameterUpdate param_id="num">source.num + 1</parameterUpdate>
		</successor>
		<successor id="B"></successor>
	</rule>

	<rule source="B">
		<successor id="A"></successor>
	</rule>
</lsml>
```
It can be accessed by following method

```C#
public static void Main(string[] args)
{
    var lSys = new XmlLSystemProvider().Load("l-system.xml").Compile();
    lSys.Init();
    Display(lSys.CurrentState);
    for (var i = 0; i < 5; i++)
    {
        lSys.Step();
        Display(lSys.CurrentState);
    }
}

private static void Display(IEnumerable<IModule> state)
{
    foreach (var m in state)
    {
        if (m.parameters.Count() == 0)
            Console.Write($"{m.Id}; ");
        else
        {
            Console.Write($"{m.Id}({string.Join(", ", m.parameters)}); ");
        }
    }
    Console.WriteLine();
}

//B;
//A(num=1);
//A(num=2); B;
//A(num=3); B; A(num=1);
//A(num=4); B; A(num=1); A(num=2); B;
//A(num=5); B; A(num=1); A(num=2); B; A(num=3); B; A(num=1);
```

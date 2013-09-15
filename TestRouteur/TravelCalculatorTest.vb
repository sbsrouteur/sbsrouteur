Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Routeur



'''<summary>
'''Classe de test pour TravelCalculatorTest, destinée à contenir tous
'''les tests unitaires TravelCalculatorTest
'''</summary>
<TestClass()> _
Public Class TravelCalculatorTest


    Private testContextInstance As TestContext

    '''<summary>
    '''Obtient ou définit le contexte de test qui fournit
    '''des informations sur la série de tests active ainsi que ses fonctionnalités.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(ByVal value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "Attributs de tests supplémentaires"
    '
    'Vous pouvez utiliser les attributs supplémentaires suivants lors de l'écriture de vos tests :
    '
    'Utilisez ClassInitialize pour exécuter du code avant d'exécuter le premier test dans la classe
    '<ClassInitialize()>  _
    'Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    'End Sub
    '
    'Utilisez ClassCleanup pour exécuter du code après que tous les tests ont été exécutés dans une classe
    '<ClassCleanup()>  _
    'Public Shared Sub MyClassCleanup()
    'End Sub
    '
    'Utilisez TestInitialize pour exécuter du code avant d'exécuter chaque test
    '<TestInitialize()>  _
    'Public Sub MyTestInitialize()
    'End Sub
    '
    'Utilisez TestCleanup pour exécuter du code après que chaque test a été exécuté
    '<TestCleanup()>  _
    'Public Sub MyTestCleanup()
    'End Sub
    '
#End Region

    <TestMethod()> _
    Public Sub OrthoAngle_Test1()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(0, 0)
        target.EndPoint = New Coords(0, 10)


        Dim expected As Double = 90 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.OrthoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()> _
    Public Sub OrthoAngle_Test2()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, 175)
        target.EndPoint = New Coords(-50, -155)


        Dim expected As Double = 133.80076801438372 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.OrthoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()> _
    Public Sub OrthoAngle_Test3()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, -175)
        target.EndPoint = New Coords(-50, -155)


        Dim expected As Double = 141.96670437996249 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.OrthoCourse_Deg
        Assert.IsTrue(Math.Abs(expected - actual) < 0.000000001)

    End Sub


    <TestMethod()> _
    Public Sub LoxoAngle_Test1()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, 175)
        target.EndPoint = New Coords(-35, -175)


        Dim expected As Double = 90 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.LoxoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()> _
    Public Sub LoxoAngle_Test2()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, -175)
        target.EndPoint = New Coords(-35, 175)


        Dim expected As Double = 270 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.LoxoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()> _
    Public Sub LoxoAngle_Test3()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, 105)
        target.EndPoint = New Coords(-35, 175)


        Dim expected As Double = 90 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.LoxoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()> _
    Public Sub LoxoAngle_Test4()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, 155)
        target.EndPoint = New Coords(-35, 105)


        Dim expected As Double = 270 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.LoxoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub

    <TestMethod()> _
    Public Sub LoxoAngle_Test5()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        target.StartPoint = New Coords(-35, 175)
        target.EndPoint = New Coords(-50, -155)


        Dim expected As Double = 124.35013371889738 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.LoxoCourse_Deg
        Assert.AreEqual(expected, actual)

    End Sub



    '''<summary>
    '''Test pour ReachDistance
    '''</summary>
    <TestMethod()> _
    Public Sub ReachDistanceTest()
        Dim target As TravelCalculator = New TravelCalculator ' TODO : initialisez à une valeur appropriée
        Dim Dist As Double = 0.0! ' TODO : initialisez à une valeur appropriée
        Dim tc_deg As Double = 0.0! ' TODO : initialisez à une valeur appropriée
        Dim expected As Coords = Nothing ' TODO : initialisez à une valeur appropriée
        Dim actual As Coords
        actual = target.ReachDistance(Dist, tc_deg)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Vérifiez l'exactitude de cette méthode de test.")
    End Sub


End Class

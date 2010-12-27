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


    '''<summary>
    '''Test pour ReachDistanceVLM
    '''</summary>
    <TestMethod(), _
     DeploymentItem("Routeur.exe")> _
    Public Sub ReachDistanceVLMTest()
        Dim target As TravelCalculator_Accessor = New TravelCalculator_Accessor ' TODO : initialisez à une valeur appropriée
        Dim Dist As Double = 0.0! ' TODO : initialisez à une valeur appropriée
        Dim tc_deg As Double = 0.0! ' TODO : initialisez à une valeur appropriée
        Dim expected As Coords = Nothing ' TODO : initialisez à une valeur appropriée
        Dim actual As Coords
        actual = target.ReachDistanceVLM(Dist, tc_deg)
        Assert.AreEqual(expected, actual)
        Assert.Inconclusive("Vérifiez l'exactitude de cette méthode de test.")
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

    <TestMethod()> _
    Public Sub TestRaechDistMethods()

        Dim TC1 As New TravelCalculator_Accessor
        Dim TC2 As New TravelCalculator_Accessor
        Dim TC3 As New TravelCalculator
        TC1.StartPoint = New Coords(45, 0)
        TC2.StartPoint = New Coords(45, 0)
        Dim LoopCount As Integer = 1
        Dim T1 As DateTime
        Dim t2 As DateTime
        Dim T3 As DateTime
        Dim TC1Span As New TimeSpan(0)
        Dim tc2span As New TimeSpan(0)

        While LoopCount < 50010
            T1 = Now
            Dim Ret1 As Coords = TC1.ReachDistanceAviat(4, 105)
            t2 = Now
            Dim Ret2 As Coords = TC2.ReachDistanceVLM(4, 105)
            T3 = Now
            'TC3.StartPoint = Ret1
            'TC3.EndPoint = Ret2
            'Assert.IsTrue(Ret1.ToString = Ret2.ToString, Ret1.ToString & "<>" & Ret2.ToString & "@" & LoopCount & Ret1.Lon_Deg & " " & Ret2.Lon_Deg)
            TC1Span = TC1Span.Add(t2.Subtract(T1))
            tc2span = tc2span.Add(T3.Subtract(t2))
            TC1.StartPoint = Ret1
            TC2.StartPoint = Ret2
            LoopCount += 1
            'Assert.IsTrue(Ret1.Lon = Ret2.Lon, Ret1.ToString & "<>" & Ret2.ToString)
        End While

        Assert.IsTrue(False, TC1Span.ToString & " " & tc2span.ToString)

    End Sub

End Class

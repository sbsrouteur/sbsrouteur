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

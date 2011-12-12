Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Routeur



'''<summary>
'''Classe de test pour GSHHS_UtilsTest, destinée à contenir tous
'''les tests unitaires GSHHS_UtilsTest
'''</summary>
<TestClass()> _
Public Class GSHHS_UtilsTest


    Private testContextInstance As TestContext

    '''<summary>
    '''Obtient ou définit le contexte de test qui fournit
    '''des informations sur la série de tests active ainsi que ses fonctionnalités.
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "Attributs de tests supplémentaires"
    '
    'Vous pouvez utiliser les attributs supplémentaires suivants lorsque vous écrivez vos tests :
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
    '''Test pour GetLineCoefs
    '''</summary>
    <TestMethod(), _
     DeploymentItem("Routeur.exe")> _
    Public Sub GetLineCoefsTest1()
        Dim P1 As Coords = New Coords(0, 0)
        Dim P2 As Coords = New Coords(1 / Math.PI * 180, 1 / Math.PI * 180)
        Dim expected As Coords = New Coords(1 / Math.PI * 180, 0)
        Dim actual As GSHHS_Utils_Accessor.LineParam
        actual = GSHHS_Utils_Accessor.GetLineCoefs(P1, P2)
        Assert.AreEqual(expected.N_Lat, actual.a, 0.00000001)
        Assert.AreEqual(expected.N_Lon, actual.b, 0.00000001)
    End Sub
    <TestMethod(), _
         DeploymentItem("Routeur.exe")> _
    Public Sub GetLineCoefsTest2()
        Dim P1 As Coords = New Coords(10, 0)
        Dim P2 As Coords = New Coords(10, 1 / Math.PI * 180)
        Dim expected As LineParam
        expected.a = 0
        expected.b = 10
        Dim actual As GSHHS_Utils_Accessor.LineParam
        actual = GSHHS_Utils_Accessor.GetLineCoefs(P1, P2)
        Assert.AreEqual(expected.a, actual.a, 0.00000001)
        Assert.AreEqual(expected.b, actual.b, 0.00000001)
    End Sub

    <TestMethod(), _
     DeploymentItem("Routeur.exe")> _
    Public Sub GetLineCoefsTest3()
        Dim P1 As Coords = New Coords(0, 1 / Math.PI * 180)
        Dim P2 As Coords = New Coords(10, 1 / Math.PI * 180)
        Dim actual As GSHHS_Utils_Accessor.LineParam
        actual = GSHHS_Utils_Accessor.GetLineCoefs(P1, P2)
        Assert.AreEqual(Double.NaN, actual.a, 0.00000001)
        Assert.AreEqual(1, actual.b, 0.00000001)
    End Sub

    '''<summary>
    '''Test pour IntersectSegments
    '''</summary>
    <TestMethod()> _
    Public Sub IntersectSegmentsTest1()
        Dim S1_P1 As Coords = New Coords(0, 0)
        Dim S1_P2 As Coords = New Coords(1, 0)
        Dim S2_P1 As Coords = New Coords(0.5, -1)
        Dim S2_P2 As Coords = New Coords(0.5, 1)
        Dim expected As Boolean = True
        Dim actual As Boolean
        actual = GSHHS_Utils.IntersectSegments(S1_P1, S1_P2, S2_P1, S2_P2)
        Assert.AreEqual(expected, actual)

    End Sub

    '''<summary>
    '''Test pour IntersectSegments
    '''</summary>
    <TestMethod()> _
    Public Sub IntersectSegmentsTest2()
        Dim S1_P1 As Coords = New Coords(0, 0)
        Dim S1_P2 As Coords = New Coords(1, 0)
        Dim S2_P1 As Coords = New Coords(0.5, 0)
        Dim S2_P2 As Coords = New Coords(0.5, 1)
        Dim expected As Boolean = True
        Dim actual As Boolean
        actual = GSHHS_Utils.IntersectSegments(S1_P1, S1_P2, S2_P1, S2_P2)
        Assert.AreEqual(expected, actual)

    End Sub

    '''<summary>
    '''Test pour IntersectSegments
    '''</summary>
    <TestMethod()> _
    Public Sub IntersectSegmentsTest3()
        Dim S1_P1 As Coords = New Coords(0, 0)
        Dim S1_P2 As Coords = New Coords(1, 0)
        Dim S2_P1 As Coords = New Coords(0.5, -1)
        Dim S2_P2 As Coords = New Coords(0.5, -2)
        Dim expected As Boolean = True
        Dim actual As Boolean
        actual = GSHHS_Utils.IntersectSegments(S1_P1, S1_P2, S2_P1, S2_P2)
        Assert.AreEqual(expected, actual)

    End Sub

    '''<summary>
    '''Test pour IntersectSegments
    '''</summary>
    <TestMethod()> _
    Public Sub IntersectSegmentsTest4()
        Dim S1_P1 As Coords = New Coords(33,54,33,Coords.NORTH_SOUTH.S,18,22,06,Coords.EAST_WEST.E)
        Dim S1_P2 As Coords = New Coords(33, 55, 21, Coords.NORTH_SOUTH.S, 18, 23, 4, Coords.EAST_WEST.E)
        Dim S2_P1 As Coords = New Coords(33, 55, 0, Coords.NORTH_SOUTH.S, 18, 22, 37, Coords.EAST_WEST.E)
        Dim S2_P2 As Coords = New Coords(33, 55, 56, Coords.NORTH_SOUTH.S, 18, 22, 37, Coords.EAST_WEST.E)
        Dim expected As Boolean = True
        Dim actual As Boolean
        actual = GSHHS_Utils.IntersectSegments(S1_P1, S1_P2, S2_P1, S2_P2)
        Assert.AreEqual(expected, actual)

    End Sub
    '''<summary>
    '''Test pour IntersectSegments
    '''</summary>
    <TestMethod()> _
    Public Sub IntersectSegmentsTest5()
        Dim S1_P1 As Coords = New Coords(33, 2, 56, Coords.NORTH_SOUTH.S, 27, 52, 17, Coords.EAST_WEST.E)
        Dim S1_P2 As Coords = New Coords(33, 2, 13, Coords.NORTH_SOUTH.S, 27, 52, 17, Coords.EAST_WEST.E)
        Dim S2_P1 As Coords = New Coords(33, 3, 23, Coords.NORTH_SOUTH.S, 27, 53, 18, Coords.EAST_WEST.E)
        Dim S2_P2 As Coords = New Coords(33, 2, 59, Coords.NORTH_SOUTH.S, 27, 52, 3, Coords.EAST_WEST.E)
        Dim expected As Boolean = True
        Dim actual As Boolean
        actual = GSHHS_Utils.IntersectSegments(S1_P1, S1_P2, S2_P1, S2_P2)
        Assert.AreEqual(expected, actual)

    End Sub

End Class

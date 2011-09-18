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
        Dim actual As Coords
        actual = GSHHS_Utils_Accessor.GetLineCoefs(P1, P2)
        Assert.AreEqual(expected.N_Lat, actual.N_Lat, 0.00000001)
        Assert.AreEqual(expected.N_Lon, actual.N_Lon, 0.00000001)
    End Sub
    <TestMethod(), _
         DeploymentItem("Routeur.exe")> _
    Public Sub GetLineCoefsTest2()
        Dim P1 As Coords = New Coords(10, 0)
        Dim P2 As Coords = New Coords(10, 1 / Math.PI * 180)
        Dim expected As Coords = New Coords(0, 10)
        Dim actual As Coords
        actual = GSHHS_Utils_Accessor.GetLineCoefs(P1, P2)
        Assert.AreEqual(expected.N_Lat, actual.N_Lat, 0.00000001)
        Assert.AreEqual(expected.N_Lon, actual.N_Lon, 0.00000001)
    End Sub

    <TestMethod(), _
     DeploymentItem("Routeur.exe")> _
    Public Sub GetLineCoefsTest3()
        Dim P1 As Coords = New Coords(0, 1 / Math.PI * 180)
        Dim P2 As Coords = New Coords(10, 1 / Math.PI * 180)
        Dim actual As Coords
        actual = GSHHS_Utils_Accessor.GetLineCoefs(P1, P2)
        Assert.AreEqual(Double.NaN, actual.N_Lat, 0.00000001)
        Assert.AreEqual(1, actual.N_Lon, 0.00000001)
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
End Class

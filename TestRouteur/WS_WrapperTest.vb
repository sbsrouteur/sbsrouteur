Imports System.Collections.Generic

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Routeur



'''<summary>
'''Classe de test pour WS_WrapperTest, destinée à contenir tous
'''les tests unitaires WS_WrapperTest
'''</summary>
<TestClass()> _
Public Class WS_WrapperTest


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
    '''Test pour GetRankings
    '''</summary>
    <TestMethod()> _
    Public Sub GetRankingsTest()
        Dim RaceList() As Integer = New Integer() {81, 20100216, 20100213}
        Dim ArrivedCount As Integer
        WS_Wrapper.SetCredential("test", "test")
        For Each Race As Integer In RaceList
            Dim Res = WS_Wrapper.GetRankings(Race, ArrivedCount)

            Console.WriteLine()

        Next
        
    End Sub
End Class

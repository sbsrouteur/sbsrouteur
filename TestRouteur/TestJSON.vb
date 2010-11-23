Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Routeur
Imports System.IO
Imports System.Collections.Generic

<TestClass()> _
Public Class TestJSON

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
            testContextInstance = value
        End Set
    End Property


    <TestMethod()> _
    Public Sub TestJSonParse()

        Dim File As New StreamReader("..\..\Test Json Error.txt", FileMode.Open)
        Dim Input As String = File.ReadToEnd

        Dim Ret As Dictionary(Of String, Object) = JSonParser.Parse(Input)
        Assert.IsTrue(True)

    End Sub

End Class

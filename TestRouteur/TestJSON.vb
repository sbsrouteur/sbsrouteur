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


   
End Class

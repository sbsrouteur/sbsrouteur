Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Routeur



'''<summary>
'''Classe de test pour clsSailManagerTest, destinée à contenir tous
'''les tests unitaires clsSailManagerTest
'''</summary>
<TestClass()> _
Public Class clsSailManagerTest


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
    '''Test pour GetSpeed
    '''</summary>
    <TestMethod()> _
    Public Sub GetSpeedTestC5G3_100d_30Kts()
        Dim target As clsSailManager = New clsSailManager ' TODO : initialisez à une valeur appropriée
        Dim BoatType As String = "boat_C5g3" ' TODO : initialisez à une valeur appropriée
        Dim SailMode As clsSailManager.EnumSail = clsSailManager.EnumSail.OneSail  ' TODO : initialisez à une valeur appropriée
        Dim WindAngle As Double = 100.0 ' TODO : initialisez à une valeur appropriée
        Dim WindSpeed As Double = 30.0 ' TODO : initialisez à une valeur appropriée
        Dim expected As Double = 33.12 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.GetSpeed(BoatType, SailMode, WindAngle, WindSpeed)
        Assert.AreEqual(expected, actual)
    End Sub

    <TestMethod()> _
        Public Sub GetSpeedTestC5G3_140d_43Kts()
        Dim target As clsSailManager = New clsSailManager ' TODO : initialisez à une valeur appropriée
        Dim BoatType As String = "boat_C5g3" ' TODO : initialisez à une valeur appropriée
        Dim SailMode As clsSailManager.EnumSail = clsSailManager.EnumSail.OneSail  ' TODO : initialisez à une valeur appropriée
        Dim WindAngle As Double = 140.0 ' TODO : initialisez à une valeur appropriée
        Dim WindSpeed As Double = 43.0 ' TODO : initialisez à une valeur appropriée
        Dim expected As Double = 33.12 ' TODO : initialisez à une valeur appropriée
        Dim actual As Double
        actual = target.GetSpeed(BoatType, SailMode, WindAngle, WindSpeed)
        Assert.AreEqual(expected, actual)
    End Sub
End Class

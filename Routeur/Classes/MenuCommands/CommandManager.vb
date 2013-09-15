''===================================================================================
'' Microsoft patterns & practices
'' Composite Application Guidance for Windows Presentation Foundation and Silverlight
''===================================================================================
'' Copyright (c) Microsoft Corporation.  All rights reserved.
'' THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
'' OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
'' LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
'' FITNESS FOR A PARTICULAR PURPOSE.
''===================================================================================
'' The example companies, organizations, products, domain names,
'' e-mail addresses, logos, people, places, and events depicted
'' herein are fictitious.  No association with any real company,
'' organization, product, domain name, email address, logo, person,
'' places, or events is intended or should be inferred.
''===================================================================================
Imports System
Imports System.Windows
Imports System.Windows.Input
Imports System.Windows.Threading

Namespace Commands

    ''' <summary>
    ''' An <see cref="ICommand"/> whose delegates can be attached for .
    ''' </summary>
    ''' <typeparam name="T">Parameter type.</typeparam>
    Partial Public Class DelegateCommand(Of T)

        Implements ICommand

        Private executeMethod As Action(Of T)
        Private canExecuteMethod As Func(Of T, Boolean)
        Private _isActive As Boolean

        ''' <summary>
        ''' Initializes a new instance of ICommand.
        ''' </summary>
        ''' <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        ''' <remarks><seealso cref="CanExecute"/> will always return true.</remarks>
        Public Sub New(ByVal executeMethod As Action(Of T))
            Me.New(executeMethod, Nothing)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of ICommand.
        ''' </summary>
        ''' <param name="executeMethod">Delegate to execute when Execute is called on the command.  This can be null to just hook up a CanExecute delegate.</param>
        ''' <param name="canExecuteMethod">Delegate to execute when CanExecute is called on the command.  This can be null.</param>
        ''' <exception cref="ArgumentNullException">When both <paramref name="executeMethod"/> and <paramref name="canExecuteMethod"/> ar <see langword="null" />.</exception>
        Public Sub New(ByVal executeMethod As Action(Of T), ByVal canExecuteMethod As Func(Of T, Boolean))
            If executeMethod Is Nothing And canExecuteMethod Is Nothing Then
                Throw New ArgumentNullException("executeMethod Delegates cannot be NULL")
            End If
            Me.executeMethod = executeMethod
            Me.canExecuteMethod = canExecuteMethod

        End Sub

        '''<summary>
        '''Defines the method that determines whether the command can execute in its current state.
        '''</summary>
        '''<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        '''<returns>
        '''<see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
        '''</returns>
        Public Function CanExecute(ByVal parameter As T) As Boolean

            If canExecuteMethod Is Nothing Then Return True
            Return canExecuteMethod(parameter)
        End Function

        '''<summary>
        '''Defines the method to be called when the command is invoked.
        '''</summary>
        '''<param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to <see langword="null" />.</param>
        Public Sub Execute(ByVal parameter As T)
            If (executeMethod Is Nothing) Then Return
            executeMethod(parameter)
        End Sub

        '''<summary>
        '''Defines the method that determines whether the command can execute in its current state.
        '''</summary>
        '''<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        '''<returns>
        '''true if this command can be executed; otherwise, false.
        '''</returns>
        Private Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute

            Return CanExecute(CType(parameter, T))
        End Function

        '''<summary>
        '''Occurs when changes occur that affect whether or not the command should execute.
        '''</summary>
        Public Event CanExecuteChanged(ByVal sender As Object, ByVal e As EventArgs) Implements ICommand.CanExecuteChanged

        '''<summary>
        '''Defines the method to be called when the command is invoked.
        '''</summary>
        '''<param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        Private Sub Execute(ByVal parameter As Object) Implements ICommand.Execute

            Execute(CType(parameter, T))
        End Sub

        ''' <summary>
        ''' Raises <see cref="CanExecuteChanged"/> on the UI thread so every command invoker
        ''' can requery to check if the command can execute.
        ''' <remarks>Note that this will trigger the execution of <see cref="CanExecute"/> once for each invoker.</remarks>
        ''' </summary>
        <System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")> _
        Public Sub RaiseCanExecuteChanged()

            OnCanExecuteChanged()
        End Sub

        ''' <summary>
        ''' Raises <see cref="CanExecuteChanged"/> on the UI thread so every command invoker can requery to check if the command can execute.
        ''' </summary>
        Protected Overridable Sub OnCanExecuteChanged()

            Dim dispatcher As Dispatcher = Nothing

            If (Application.Current IsNot Nothing) Then

                dispatcher = Application.Current.Dispatcher
            End If

            'If (CanExecuteHandler IsNot null) Then

            If (dispatcher IsNot Nothing And Not dispatcher.CheckAccess()) Then

                dispatcher.BeginInvoke(DispatcherPriority.Normal, _
                                       New action(AddressOf OnCanExecuteChanged))

            Else

                RaiseEvent CanExecuteChanged(Me, EventArgs.Empty)

            End If
            'End If
        End Sub

    End Class
End Namespace
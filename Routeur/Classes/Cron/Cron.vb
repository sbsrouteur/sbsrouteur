'This file is part of Routeur.
'Copyright (C) 2011  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports System.ComponentModel
Imports System.Windows.Threading

Public Class Cron

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private Property Dispatcher As Dispatcher
    Private Property CronTimer As DispatcherTimer
    Private Property Tasks As New List(Of CronTask)

    Public Sub New(ByVal Dispatcher As Windows.Threading.Dispatcher)

        Me.Dispatcher = Dispatcher
        CronTimer = New DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        AddHandler CronTimer.Tick, AddressOf DispatcherTick
        CronTimer.Interval = New TimeSpan(0, 0, 1)
        CronTimer.Start()

    End Sub

    Private Sub DispatcherTick(ByVal Sender As Object, ByVal e As EventArgs)
        CronTimer.Stop()
        For Each task In Tasks
            If Now > task.NextStartTime Then
                task.RunTask()
                task.NextStartTime = task.NextStartTime.Add(task.Period)
            End If
        Next
        CronTimer.Start()
    End Sub

    Sub ClearTasks()
        Tasks.Clear()
    End Sub

    Sub [Stop]()

        CronTimer.Stop()
    End Sub

    Sub Start()
        CronTimer.Start()
    End Sub

End Class

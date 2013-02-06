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


Public Class CronTask

    Public Property Period As TimeSpan
    Public Property NextStartTime As DateTime
    Public Delegate Sub CronTaskRunner()

    Private _Task As CronTaskRunner

    Public Sub New(ByVal Task As CronTaskRunner)
        _Task = Task
    End Sub

    Public Sub RunTask()

        _Task()

    End Sub
    
End Class

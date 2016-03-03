'
' Created by SharpDevelop.
' User: Аццкий_хакер
' Date: 08.04.2011
' Time: 17:14
' 
' To change this template use Tools | Options | Coding | Edit Standard Headers.
'
Partial Class MainForm
	Inherits System.Windows.Forms.Form
	
	''' <summary>
	''' Designer variable used to keep track of non-visual components.
	''' </summary>
	Private components As System.ComponentModel.IContainer
	
	''' <summary>
	''' Disposes resources used by the form.
	''' </summary>
	''' <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		If disposing Then
			If components IsNot Nothing Then
				components.Dispose()
			End If
		End If
		MyBase.Dispose(disposing)
	End Sub
	
	''' <summary>
	''' This method is required for Windows Forms designer support.
	''' Do not change the method contents inside the source code editor. The Forms designer might
	''' not be able to load this method if it was changed manually.
	''' </summary>
	Private Sub InitializeComponent()
		Me.button1 = New System.Windows.Forms.Button
		Me.textBox1 = New System.Windows.Forms.TextBox
		Me.label1 = New System.Windows.Forms.Label
		Me.button2 = New System.Windows.Forms.Button
		Me.SuspendLayout
		'
		'button1
		'
		Me.button1.Location = New System.Drawing.Point(79, 47)
		Me.button1.Name = "button1"
		Me.button1.Size = New System.Drawing.Size(121, 23)
		Me.button1.TabIndex = 0
		Me.button1.Text = "Дать"
		Me.button1.UseVisualStyleBackColor = true
		AddHandler Me.button1.Click, AddressOf Me.Button1Click
		'
		'textBox1
		'
		Me.textBox1.Location = New System.Drawing.Point(12, 21)
		Me.textBox1.Name = "textBox1"
		Me.textBox1.Size = New System.Drawing.Size(268, 20)
		Me.textBox1.TabIndex = 1
		AddHandler Me.textBox1.TextChanged, AddressOf Me.TextBox1TextChanged
		'
		'label1
		'
		Me.label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(204,Byte))
		Me.label1.ForeColor = System.Drawing.Color.Red
		Me.label1.Location = New System.Drawing.Point(12, 73)
		Me.label1.Name = "label1"
		Me.label1.Size = New System.Drawing.Size(269, 42)
		Me.label1.TabIndex = 2
		Me.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
		'
		'button2
		'
		Me.button2.Location = New System.Drawing.Point(79, 118)
		Me.button2.Name = "button2"
		Me.button2.Size = New System.Drawing.Size(121, 23)
		Me.button2.TabIndex = 3
		Me.button2.Text = "Взять"
		Me.button2.UseVisualStyleBackColor = true
		AddHandler Me.button2.Click, AddressOf Me.Button2Click
		'
		'MainForm
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(293, 166)
		Me.Controls.Add(Me.button2)
		Me.Controls.Add(Me.label1)
		Me.Controls.Add(Me.textBox1)
		Me.Controls.Add(Me.button1)
		Me.MaximizeBox = false
		Me.Name = "MainForm"
		Me.ShowIcon = false
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "Давлка =)"
		Me.ResumeLayout(false)
		Me.PerformLayout
	End Sub
	Private button2 As System.Windows.Forms.Button
	Private label1 As System.Windows.Forms.Label
	Private textBox1 As System.Windows.Forms.TextBox
	Private button1 As System.Windows.Forms.Button
End Class

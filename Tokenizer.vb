﻿Imports System.IO
Imports System.Text.RegularExpressions
Namespace Basic_NLP
    Public Module Extensions
        <Runtime.CompilerServices.Extension()>
        Public Function ExtractStringBetween(ByVal value As String, ByVal strStart As String, ByVal strEnd As String) As String
            If Not String.IsNullOrEmpty(value) Then
                Dim i As Integer = value.IndexOf(strStart)
                Dim j As Integer = value.IndexOf(strEnd)
                Return value.Substring(i, j - i)
            Else
                Return value
            End If
        End Function
        Public Function ReadTextFilesFromDirectory(directoryPath As String) As List(Of String)
            Dim fileList As New List(Of String)()

            Try
                Dim txtFiles As String() = Directory.GetFiles(directoryPath, "*.txt")

                For Each filePath As String In txtFiles
                    Dim fileContent As String = File.ReadAllText(filePath)
                    fileList.Add(fileContent)
                Next
            Catch ex As Exception
                ' Handle any exceptions that may occur while reading the files.
                Console.WriteLine("Error: " & ex.Message)
            End Try

            Return fileList
        End Function

        ''' <summary>
        ''' Writes the contents of an embedded resource embedded as Bytes to disk.
        ''' </summary>
        ''' <param name="BytesToWrite">Embedded resource</param>
        ''' <param name="FileName">    Save to file</param>
        ''' <remarks></remarks>
        <System.Runtime.CompilerServices.Extension()>
        Public Sub ResourceFileSave(ByVal BytesToWrite() As Byte, ByVal FileName As String)

            If IO.File.Exists(FileName) Then
                IO.File.Delete(FileName)
            End If

            Dim FileStream As New System.IO.FileStream(FileName, System.IO.FileMode.OpenOrCreate)
            Dim BinaryWriter As New System.IO.BinaryWriter(FileStream)

            BinaryWriter.Write(BytesToWrite)
            BinaryWriter.Close()
            FileStream.Close()
        End Sub

        <Runtime.CompilerServices.Extension()>
        Public Function SpaceItems(ByRef txt As String, Item As String) As String
            Return txt.Replace(Item, " " & Item & " ")
        End Function
    End Module
    Public Class Tokenizer
        Public Shared ReadOnly CodePunctuation() As String = {"\", "#", "@", "^"}

        Public Shared Delimiters() As Char = {CType(" ", Char), CType(".", Char),
                    CType(",", Char), CType("?", Char),
                    CType("!", Char), CType(";", Char),
                    CType(":", Char), Chr(10), Chr(13), vbTab}
        Public NGramSize As Integer = 2



        Private Shared ReadOnly AlphaBet() As String = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N",
    "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n",
    "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"}

        Private Shared ReadOnly EncapuslationPunctuationEnd() As String = {"}", "]", ">", ")"}

        Private Shared ReadOnly EncapuslationPunctuationStart() As String = {"{", "[", "<", "("}

        Private Shared ReadOnly GramaticalPunctuation() As String = {".", "?", "!", ":", ";", ","}

        Private Shared ReadOnly MathPunctuation = New String() {"+", "-", "*", "/", "=", "<", ">", "≤", "≥", "±", "≈", "≠", "%", "‰", "‱", "^", "_", "√", "∛", "∜", "∫", "∬", "∭", "∮", "∯", "∰", "∇", "∂", "∆", "∏", "∑", "∐", "⨀", "⨁", "⨂", "⨃", "⨄", "∫", "∬", "∭", "∮", "∯", "∰", "∇", "∂", "∆", "∏", "∑", "∐", "⨀", "⨁", "⨂", "⨃", "⨄"}

        Private Shared ReadOnly MoneyPunctuation() As String = {"$", "€", "£", "¥", "₹", "₽", "₿"}

        Private Shared ReadOnly Number() As String = {"1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
"30", "40", "50", "60", "70", "80", "90", "00", "000", "0000", "00000", "000000", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen",
"nineteen", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety", "hundred", "thousand", "million", "Billion"}

        Private Shared ReadOnly SeperatorPunctuation() As String = {" ", ",", "|"}

        Private Shared ReadOnly Symbols() As String = {"@", "#", "$", "%", "&", "*", "+", "=", "^", "_", "~", "§", "°", "¿", "¡"}

        Private Shared centralResource As New Dictionary(Of String, List(Of String))

        Private Shared Param_Freq As Integer = 4

        Private iStopWords As New List(Of String)

        Private ModelType As TokenizerType



        Private VocabularyWithFrequency As New Dictionary(Of String, Integer)

        Public Sub New(ByRef Model As Tokenizer)

            Me.VocabularyWithFrequency = Model.VocabularyWithFrequency

            Me.iStopWords = Model.iStopWords

            Me.StopWords = Model.StopWords
            Me.ModelType = Model.ModelType
            Me.StopWordRemovalEnabled = Model.StopWordRemovalEnabled
        End Sub

        Public Sub New(ByRef Modeltype As TokenizerType)
            Me.ModelType = Modeltype
        End Sub

        Public Enum TokenizationOption
            Character
            Word
            Sentence
            Sub_Word
        End Enum

        Public Enum TokenizerType
            Word
            Sentence
            Character
            Ngram
            WordGram
            SentenceGram
            BitWord
        End Enum

        Public Enum TokenType
            GramaticalPunctuation
            EncapuslationPunctuationStart
            EncapuslationPunctuationEnd
            MoneyPunctuation
            MathPunctuation
            CodePunctuation
            AlphaBet
            Number
            Symbol
            SeperatorPunctuation
            Ignore
            Word
            Sentence
            Character
            Ngram
            WordGram
            SentenceGram
            BitWord
            Punctuation
            whitespace
        End Enum

        Public ReadOnly Property RefferenceVocabulary As Dictionary(Of String, List(Of String))
            Get
                Return centralResource
            End Get
        End Property

        Public Property StopWordRemovalEnabled As Boolean

        Public Property StopWords As List(Of String)
            Get
                Return iStopWords
            End Get
            Set(value As List(Of String))
                iStopWords = value
            End Set
        End Property

        Public ReadOnly Property TokenizerVocabulary As Dictionary(Of String, Integer)
            Get
                Return VocabularyWithFrequency
            End Get
        End Property

        Private ReadOnly Property Vocabulary As List(Of String)
            Get
                Dim Lst As New List(Of String)
                For Each item In VocabularyWithFrequency
                    Lst.Add(item.Key)
                Next
                Return Lst
            End Get

        End Property

        Public Shared Function AlphanumericOnly(ByRef Str As String) As String
            Str = GetValidTokens(Str)
            Str = RemoveTokenType(Str, Tokenizer.TokenType.CodePunctuation)
            Str = RemoveTokenType(Str, Tokenizer.TokenType.EncapuslationPunctuationEnd)
            Str = RemoveTokenType(Str, Tokenizer.TokenType.EncapuslationPunctuationStart)
            Str = RemoveTokenType(Str, Tokenizer.TokenType.MathPunctuation)
            Str = RemoveTokenType(Str, Tokenizer.TokenType.MoneyPunctuation)
            Str = RemoveTokenType(Str, Tokenizer.TokenType.GramaticalPunctuation)
            Str = Str.Remove(",")
            Str = Str.Remove("|")
            Str = Str.Remove("_")
            Return Str
        End Function




        ''' <summary>
        ''' Counts tokens in string
        ''' </summary>
        ''' <param name="InputStr"></param>
        ''' <returns></returns>
        Public Shared Function CountPossibleTokens(ByRef InputStr As String) As Integer
            Dim Words() As String = Split(InputStr, " ")
            Return Words.Count
        End Function

        ''' <summary>
        ''' Counts tokens in string
        ''' </summary>
        ''' <param name="InputStr"></param>
        ''' <param name="Delimiter"></param>
        ''' <returns></returns>
        Public Shared Function CountPossibleTokens(ByRef InputStr As String, ByRef Delimiter As String) As Integer
            Dim Words() As String = Split(InputStr, Delimiter)
            Return Words.Count
        End Function


        Public Shared Function ExtractEncapsulated(ByRef Userinput As String) As String
            ExtractEncapsulated = Userinput
            If ContainsEncapsulated(ExtractEncapsulated) = True Then
                If ExtractEncapsulated.Contains("(") = True And ExtractEncapsulated.Contains(")") = True Then
                    ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("(", ")")
                End If
                If Userinput.Contains("[") = True And Userinput.Contains("]") = True Then
                    ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("[", "]")
                End If
                If Userinput.Contains("{") = True And Userinput.Contains("}") = True Then
                    ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("{", "}")
                End If
                If Userinput.Contains("<") = True And Userinput.Contains(">") = True Then
                    ExtractEncapsulated = ExtractEncapsulated.ExtractStringBetween("<", ">")
                End If
            End If
        End Function

        Public Shared Function ExtractStringBetween(ByVal value As String, ByVal strStart As String, ByVal strEnd As String) As String
            If Not String.IsNullOrEmpty(value) Then
                Dim i As Integer = value.IndexOf(strStart)
                Dim j As Integer = value.IndexOf(strEnd)
                Return value.Substring(i, j - i)
            Else
                Return value
            End If
        End Function

        Public Shared Function FindFrequentBigrams(words As List(Of String)) As List(Of String)
            Dim bigramCounts As New Dictionary(Of String, Integer)

            For i As Integer = 0 To words.Count - 2
                Dim bigram As String = words(i) & " " & words(i + 1)

                If bigramCounts.ContainsKey(bigram) Then
                    bigramCounts(bigram) += 1
                Else
                    bigramCounts.Add(bigram, 1)
                End If
            Next

            Dim frequentBigrams As New List(Of String)

            For Each pair In bigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentBigrams.Add(pair.Key)
                End If
            Next

            Return frequentBigrams
        End Function

        Public Shared Function FindFrequentCharacterBigrams(words As List(Of String)) As List(Of String)
            Dim bigramCounts As New Dictionary(Of String, Integer)

            For Each word In words
                Dim characters As Char() = word.ToCharArray()

                For i As Integer = 0 To characters.Length - 2
                    Dim bigram As String = characters(i) & characters(i + 1)

                    If bigramCounts.ContainsKey(bigram) Then
                        bigramCounts(bigram) += 1
                    Else
                        bigramCounts.Add(bigram, 1)
                    End If
                Next
            Next

            Dim frequentCharacterBigrams As New List(Of String)

            For Each pair In bigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentCharacterBigrams.Add(pair.Key)
                End If
            Next

            Return frequentCharacterBigrams
        End Function

        Public Shared Function FindFrequentCharacterTrigrams(words As List(Of String)) As List(Of String)
            Dim trigramCounts As New Dictionary(Of String, Integer)

            For Each word In words
                Dim characters As Char() = word.ToCharArray()

                For i As Integer = 0 To characters.Length - 3
                    Dim trigram As String = characters(i) & characters(i + 1) & characters(i + 2)

                    If trigramCounts.ContainsKey(trigram) Then
                        trigramCounts(trigram) += 1
                    Else
                        trigramCounts.Add(trigram, 1)
                    End If
                Next
            Next

            Dim frequentCharacterTrigrams As New List(Of String)

            For Each pair In trigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentCharacterTrigrams.Add(pair.Key)
                End If
            Next

            Return frequentCharacterTrigrams
        End Function

        Public Shared Function FindFrequentSentenceBigrams(sentences As List(Of String)) As List(Of String)
            Dim bigramCounts As New Dictionary(Of String, Integer)

            For i As Integer = 0 To sentences.Count - 2
                Dim bigram As String = sentences(i) & " " & sentences(i + 1)

                If bigramCounts.ContainsKey(bigram) Then
                    bigramCounts(bigram) += 1
                Else
                    bigramCounts.Add(bigram, 1)
                End If
            Next

            Dim frequentSentenceBigrams As New List(Of String)

            For Each pair In bigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentSentenceBigrams.Add(pair.Key)
                End If
            Next

            Return frequentSentenceBigrams
        End Function

        Public Shared Function FindFrequentSentenceTrigrams(sentences As List(Of String)) As List(Of String)
            Dim trigramCounts As New Dictionary(Of String, Integer)

            For i As Integer = 0 To sentences.Count - 3
                Dim trigram As String = sentences(i) & " " & sentences(i + 1) & " " & sentences(i + 2)

                If trigramCounts.ContainsKey(trigram) Then
                    trigramCounts(trigram) += 1
                Else
                    trigramCounts.Add(trigram, 1)
                End If
            Next

            Dim frequentSentenceTrigrams As New List(Of String)

            For Each pair In trigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentSentenceTrigrams.Add(pair.Key)
                End If
            Next

            Return frequentSentenceTrigrams
        End Function

        Public Shared Function FindFrequentTrigrams(words As List(Of String)) As List(Of String)
            Dim trigramCounts As New Dictionary(Of String, Integer)

            For i As Integer = 0 To words.Count - 3
                Dim trigram As String = words(i) & " " & words(i + 1) & " " & words(i + 2)

                If trigramCounts.ContainsKey(trigram) Then
                    trigramCounts(trigram) += 1
                Else
                    trigramCounts.Add(trigram, 1)
                End If
            Next

            Dim frequentTrigrams As New List(Of String)

            For Each pair In trigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentTrigrams.Add(pair.Key)
                End If
            Next

            Return frequentTrigrams
        End Function

        Public Shared Function FindFrequentWordBigrams(sentences As List(Of String)) As List(Of String)
            Dim bigramCounts As New Dictionary(Of String, Integer)

            For Each sentence In sentences
                Dim words As String() = sentence.Split(" "c)

                For i As Integer = 0 To words.Length - 2
                    Dim bigram As String = words(i) & " " & words(i + 1)

                    If bigramCounts.ContainsKey(bigram) Then
                        bigramCounts(bigram) += 1
                    Else
                        bigramCounts.Add(bigram, 1)
                    End If
                Next
            Next

            Dim frequentWordBigrams As New List(Of String)

            For Each pair In bigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentWordBigrams.Add(pair.Key)
                End If
            Next

            Return frequentWordBigrams
        End Function

        Public Shared Function FindFrequentWordTrigrams(sentences As List(Of String)) As List(Of String)
            Dim trigramCounts As New Dictionary(Of String, Integer)

            For Each sentence In sentences
                Dim words As String() = sentence.Split(" "c)

                For i As Integer = 0 To words.Length - 3
                    Dim trigram As String = words(i) & " " & words(i + 1) & " " & words(i + 2)

                    If trigramCounts.ContainsKey(trigram) Then
                        trigramCounts(trigram) += 1
                    Else
                        trigramCounts.Add(trigram, 1)
                    End If
                Next
            Next

            Dim frequentWordTrigrams As New List(Of String)

            For Each pair In trigramCounts
                If pair.Value > Param_Freq Then ' Adjust the threshold as needed
                    frequentWordTrigrams.Add(pair.Key)
                End If
            Next

            Return frequentWordTrigrams
        End Function



        Public Shared Function GetEncapsulated(ByRef Userinput As String) As List(Of String)
            GetEncapsulated = New List(Of String)
            Do Until ContainsEncapsulated(Userinput) = False
                GetEncapsulated.Add(ExtractEncapsulated(Userinput))
            Loop
        End Function

        Public Shared Function GetHighFreq(ByRef Vocabulary As Dictionary(Of String, Integer)) As List(Of String)
            Dim HighFreq As New List(Of String)
            For Each item In Vocabulary
                If item.Value > Param_Freq Then
                    HighFreq.Add(item.Key)
                End If
            Next
            Return HighFreq
        End Function

        Public Shared Function GetTokenType(ByRef CharStr As String) As TokenType
            For Each item In SeperatorPunctuation
                If CharStr = item Then Return TokenType.SeperatorPunctuation
            Next
            For Each item In GramaticalPunctuation
                If CharStr = item Then Return TokenType.GramaticalPunctuation
            Next
            For Each item In EncapuslationPunctuationStart
                If CharStr = item Then Return TokenType.EncapuslationPunctuationStart
            Next
            For Each item In EncapuslationPunctuationEnd
                If CharStr = item Then Return TokenType.EncapuslationPunctuationEnd
            Next
            For Each item In MoneyPunctuation
                If CharStr = item Then Return TokenType.MoneyPunctuation
            Next
            For Each item In MathPunctuation
                If CharStr = item Then Return TokenType.MathPunctuation
            Next
            For Each item In CodePunctuation
                If CharStr = item Then Return TokenType.CodePunctuation
            Next
            For Each item In AlphaBet
                If CharStr = item Then Return TokenType.AlphaBet
            Next
            For Each item In Number
                If CharStr = item Then Return TokenType.Number
            Next
            Return TokenType.Ignore
        End Function

        Public Shared Function GetValidTokens(ByRef InputStr As String) As String
            Dim EndStr As Integer = InputStr.Length
            Dim CharStr As String = ""
            For i = 0 To EndStr - 1
                If GetTokenType(InputStr(i)) <> TokenType.Ignore Then
                    CharStr = AddSuffix(CharStr, InputStr(i))
                Else

                End If
            Next
            Return CharStr
        End Function


        Public Shared Function RemoveBrackets(ByRef Txt As String) As String
            'Brackets
            Txt = Txt.Replace("(", "")
            Txt = Txt.Replace("{", "")
            Txt = Txt.Replace("}", "")
            Txt = Txt.Replace("[", "")
            Txt = Txt.Replace("]", "")
            Return Txt
        End Function

        Public Shared Function RemoveDoubleSpace(ByRef txt As String, Item As String) As String
            Return txt.Replace(Item, "  " & Item & " ")
        End Function

        Public Shared Function RemoveMathsSymbols(ByRef Txt As String) As String
            'Maths Symbols
            Txt = Txt.Replace("+", "")
            Txt = Txt.Replace("=", "")
            Txt = Txt.Replace("-", "")
            Txt = Txt.Replace("/", "")
            Txt = Txt.Replace("*", "")
            Txt = Txt.Replace("<", "")
            Txt = Txt.Replace(">", "")
            Txt = Txt.Replace("%", "")
            Return Txt
        End Function

        Public Shared Function RemovePunctuation(ByRef Txt As String) As String
            'Punctuation
            Txt = Txt.Replace(",", "")
            Txt = Txt.Replace(".", "")
            Txt = Txt.Replace(";", "")
            Txt = Txt.Replace("'", "")
            Txt = Txt.Replace("_", "")
            Txt = Txt.Replace("?", "")
            Txt = Txt.Replace("!", "")
            Txt = Txt.Replace("&", "")
            Txt = Txt.Replace(":", "")

            Return Txt
        End Function

        Public Shared Function RemoveStopWords(ByRef txt As String, ByRef StopWrds As List(Of String)) As String
            For Each item In StopWrds
                txt = txt.Replace(item, "")
            Next
            Return txt
        End Function

        Public Shared Function RemoveSymbols(ByRef Txt As String) As String
            'Basic Symbols
            Txt = Txt.Replace("£", "")
            Txt = Txt.Replace("$", "")
            Txt = Txt.Replace("^", "")
            Txt = Txt.Replace("@", "")
            Txt = Txt.Replace("#", "")
            Txt = Txt.Replace("~", "")
            Txt = Txt.Replace("\", "")
            Return Txt
        End Function

        Public Shared Function RemoveTokenType(ByRef UserStr As String, ByRef nType As TokenType) As String

            Select Case nType
                Case TokenType.GramaticalPunctuation
                    For Each item In GramaticalPunctuation
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.AlphaBet
                    For Each item In AlphaBet
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.CodePunctuation
                    For Each item In CodePunctuation
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.EncapuslationPunctuationEnd
                    For Each item In EncapuslationPunctuationEnd
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.EncapuslationPunctuationStart
                    For Each item In EncapuslationPunctuationStart
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.Ignore
                Case TokenType.MathPunctuation
                    For Each item In MathPunctuation
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.MoneyPunctuation
                    For Each item In MoneyPunctuation
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.Number
                    For Each item In Number
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next
                Case TokenType.SeperatorPunctuation
                    For Each item In SeperatorPunctuation
                        If UCase(UserStr).Contains(UCase(item)) = True Then
                            UserStr = UCase(UserStr).Remove(UCase(item))
                        End If
                    Next

            End Select
            Return UserStr
        End Function

        Public Shared Function SpacePunctuation(ByRef Txt As String) As String
            For Each item In Symbols
                Txt = SpaceItems(Txt, item)
            Next
            For Each item In EncapuslationPunctuationEnd
                Txt = SpaceItems(Txt, item)
            Next
            For Each item In EncapuslationPunctuationStart
                Txt = SpaceItems(Txt, item)
            Next
            For Each item In GramaticalPunctuation
                Txt = SpaceItems(Txt, item)
            Next
            For Each item In MathPunctuation
                Txt = SpaceItems(Txt, item)
            Next
            For Each item In MoneyPunctuation
                Txt = SpaceItems(Txt, item)
            Next
            Return Txt
        End Function

        ''' <summary>
        ''' Returns Tokens With Positions
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Shared Function TokenizeByCharacter(ByVal input As String) As List(Of Token)
            Dim characters As Char() = input.ToCharArray()
            Dim tokens As New List(Of Token)
            Dim currentPosition As Integer = 0

            For Each character As Char In characters
                Dim startPosition As Integer = currentPosition
                Dim endPosition As Integer = currentPosition
                Dim token As New Token(TokenType.Character, character.ToString(), startPosition, endPosition)
                tokens.Add(token)
                currentPosition += 1
            Next

            Return tokens
        End Function

        ''' <summary>
        ''' Returns Tokens With Positions
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Shared Function TokenizeBySentence(ByVal input As String) As List(Of Token)
            Dim sentences As String() = input.Split("."c)
            Dim tokens As New List(Of Token)
            Dim currentPosition As Integer = 0

            For Each sentence As String In sentences
                Dim startPosition As Integer = currentPosition
                Dim endPosition As Integer = currentPosition + sentence.Length - 1
                Dim token As New Token(TokenType.Sentence, sentence, startPosition, endPosition)
                tokens.Add(token)
                currentPosition = endPosition + 2 ' Account for the period and the space after the sentence
            Next

            Return tokens
        End Function

        ''' <summary>
        ''' Returns Tokens With Positions
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Shared Function TokenizeByWord(ByVal input As String) As List(Of Token)
            Dim words As String() = input.Split(" "c)
            Dim tokens As New List(Of Token)
            Dim currentPosition As Integer = 0

            For Each word As String In words
                Dim startPosition As Integer = currentPosition
                Dim endPosition As Integer = currentPosition + word.Length - 1
                Dim token As New Token(TokenType.Word, word, startPosition, endPosition)
                tokens.Add(token)
                currentPosition = endPosition + 2 ' Account for the space between words
            Next

            Return tokens
        End Function

        ''' <summary>
        ''' Pure basic Tokenizer 
        ''' </summary>
        ''' <param name="Corpus"></param>
        ''' <param name="tokenizationOption">Type Of Tokenization</param>
        ''' <returns></returns>
        Public Shared Function TokenizeInput(ByRef Corpus As List(Of String), tokenizationOption As TokenizationOption) As List(Of String)
            Dim ivocabulary As New List(Of String)

            For Each Doc In Corpus
                Select Case tokenizationOption
                    Case TokenizationOption.Character
                        ivocabulary.AddRange(TokenizeToCharacter(Doc.ToLower))
                    Case TokenizationOption.Word
                        ivocabulary.AddRange(TokenizeToWords(Doc.ToLower))
                    Case TokenizationOption.Sentence
                        ivocabulary.AddRange(TokenizeToSentence(Doc.ToLower))
                    Case TokenizationOption.Sub_Word
                        ivocabulary.AddRange(TokenizeToSubword(Doc.ToLower))


                End Select
            Next

            Return ivocabulary
        End Function

        Public Shared Function TokenizeSubword(sentence As String) As List(Of String)
            Dim subwordTokens As New List(Of String)
            If sentence.Length > 3 Then
                ' Initialize the vocabulary with individual words
                Dim vocabulary As New Dictionary(Of String, Integer)()
                Dim words As String() = sentence.Split(" "c)
                For Each word In words
                    If vocabulary.ContainsKey(word) = False Then
                        vocabulary.Add(word, 1)
                    End If
                Next

                ' Apply BPE iterations
                Dim numIterations As Integer = 10 ' Set the desired number of BPE iterations
                For iteration As Integer = 1 To numIterations
                    Dim bigramCounts As New Dictionary(Of String, Integer)() ' Track the counts of each bigram
                    Dim trigramCounts As New Dictionary(Of String, Integer)() ' Track the counts of each trigram

                    ' Count the occurrences of bigrams and trigrams in the vocabulary
                    For Each word In vocabulary.Keys
                        Dim wordCharacters As Char() = word.ToCharArray()

                        ' Generate bigrams
                        For i As Integer = 0 To wordCharacters.Length - 2
                            Dim bigram As String = wordCharacters(i).ToString() & wordCharacters(i + 1).ToString()
                            If bigramCounts.ContainsKey(bigram) Then
                                bigramCounts(bigram) += 1
                            Else
                                bigramCounts(bigram) = 1
                            End If
                        Next

                        ' Generate trigrams
                        For i As Integer = 0 To wordCharacters.Length - 3
                            Dim trigram As String = wordCharacters(i).ToString() & wordCharacters(i + 1).ToString() & wordCharacters(i + 2).ToString()
                            If trigramCounts.ContainsKey(trigram) Then
                                trigramCounts(trigram) += 1
                            Else
                                trigramCounts(trigram) = 1
                            End If
                        Next
                    Next

                    ' Find the most frequent bigram and trigram
                    Dim mostFrequentBigram As String = bigramCounts.OrderByDescending(Function(x) x.Value).First().Key
                    Dim mostFrequentTrigram As String = trigramCounts.OrderByDescending(Function(x) x.Value).First().Key

                    ' If the most frequent bigram occurs at least twice, add it as a subword token
                    If bigramCounts(mostFrequentBigram) >= 2 AndAlso mostFrequentBigram.Length <= 3 Then
                        subwordTokens.Add("_" & mostFrequentBigram & "_")
                    End If

                    ' If the most frequent trigram occurs at least twice, add it as a subword token
                    If trigramCounts(mostFrequentTrigram) >= 2 AndAlso mostFrequentTrigram.Length <= 3 Then
                        subwordTokens.Add("_" & mostFrequentTrigram & "_")
                    End If

                    ' Stop the iterations if both the most frequent bigram and trigram occur less than twice or exceed three characters
                    If (bigramCounts(mostFrequentBigram) < 2 OrElse mostFrequentBigram.Length > 3) AndAlso
       (trigramCounts(mostFrequentTrigram) < 2 OrElse mostFrequentTrigram.Length > 3) Then
                        Exit For
                    End If
                Next

                ' Add the word tokens to the subword tokens list
                subwordTokens.AddRange(vocabulary.Keys)

                Return subwordTokens
            Else
                Return New List(Of String)
            End If

        End Function

        Public Shared Function TrainVocabulary(ByRef iVocabulary As Dictionary(Of String, Integer), Optional MinFreq As Integer = 2) As List(Of String)
            Dim HighFreq As List(Of String) = GetHighFreq(iVocabulary)
            Param_Freq = MinFreq
            Dim lst As New List(Of String)

            Dim bi = FindFrequentCharacterBigrams(HighFreq)
            If centralResource.ContainsKey("Training Bigrams") = False Then
                centralResource.Add("Training Bigrams", bi)

                For Each Item In bi

                    If iVocabulary.ContainsKey(Item) = False Then
                        iVocabulary.Add(Item, 1)
                        lst.Add(Item)
                    Else
                        'GetFreq
                        Dim x = iVocabulary(Item)
                        'Remove Prev
                        iVocabulary.Remove(Item)
                        'Update Freq + New
                        x += 1
                        iVocabulary.Add(Item, x)
                    End If

                Next
            Else

                centralResource.Remove("Training Bigrams")
                centralResource.Add("Training Bigrams", bi)

                For Each Item In bi

                    If iVocabulary.ContainsKey(Item) = False Then
                        iVocabulary.Add(Item, 1)
                        lst.Add(Item)
                    Else
                        'GetFreq
                        Dim x = iVocabulary(Item)
                        'Remove Prev
                        iVocabulary.Remove(Item)
                        'Update Freq + New
                        x += 1
                        iVocabulary.Add(Item, x)
                    End If

                Next
            End If

            Dim Tri = FindFrequentCharacterTrigrams(HighFreq)
            If centralResource.ContainsKey("Training TriGrams") = False Then
                centralResource.Add("Training TriGrams", Tri)
                For Each Item In Tri

                    If iVocabulary.ContainsKey(Item) = False Then
                        iVocabulary.Add(Item, 1)
                        lst.Add(Item)
                    Else
                        'GetFreq
                        Dim x = iVocabulary(Item)
                        'Remove Prev
                        iVocabulary.Remove(Item)
                        'Update Freq + New
                        x += 1
                        iVocabulary.Add(Item, x)
                    End If

                Next
            Else
                centralResource.Remove("Training TriGrams")
                centralResource.Add("Training TriGrams", Tri)

                For Each Item In Tri

                    If iVocabulary.ContainsKey(Item) = False Then
                        iVocabulary.Add(Item, 1)
                        lst.Add(Item)
                    Else
                        'GetFreq
                        Dim x = iVocabulary(Item)
                        'Remove Prev
                        iVocabulary.Remove(Item)
                        'Update Freq + New
                        x += 1
                        iVocabulary.Add(Item, x)
                    End If

                Next

            End If

            Return lst

        End Function


        Public Function ExportModel() As Tokenizer
            Return Me
        End Function



        Public Function Tokenize(ByVal input As String, Optional N As Integer = 1) As List(Of Token)
            Dim normalizedInput As String = NormalizeInput(input)
            Dim tokens As New List(Of Token)

            Select Case ModelType
                Case TokenizerType.Word
                    tokens = TokenizeByWord(normalizedInput)
                Case TokenizerType.Sentence
                    tokens = TokenizeBySentence(normalizedInput)
                Case TokenizerType.Character
                    tokens = TokenizeByCharacter(normalizedInput)
                Case TokenizerType.Ngram
                    tokens = CreateNgrams(normalizedInput, N)
                Case TokenizerType.WordGram
                    tokens = CreateNgrams(normalizedInput, N)
                Case TokenizerType.SentenceGram
                    tokens = CreateNgrams(normalizedInput, N)
                Case TokenizerType.BitWord
                    tokens = TokenizeBitWord(normalizedInput)

            End Select

            If StopWordRemovalEnabled Then
                tokens = RemoveStopWords(tokens)
            End If
            For Each item In tokens

                UpdateVocabulary(item.Value)


            Next

            Return tokens
        End Function

        ''' <summary>
        ''' (for training)
        ''' </summary>
        ''' <param name="token"></param>
        ''' <returns></returns>
        Public Function TokenizeBitWord(ByRef token As String) As List(Of Token)
            Dim tokenValue As String = token
            Dim subwordUnits As New List(Of String)
            Dim currentSubword As String = ""
            Dim Tokens As New List(Of Token)
            For i As Integer = 0 To tokenValue.Length - 1
                currentSubword &= tokenValue(i)
                If i < tokenValue.Length - 1 AndAlso Not Vocabulary.Contains(currentSubword) Then
                    Dim nextChar As Char = tokenValue(i + 1)
                    Dim combinedSubword As String = currentSubword & nextChar
                    If Vocabulary.Contains(combinedSubword) Then
                        ' Found a matching subword in the vocabulary
                        subwordUnits.Add(currentSubword)
                        currentSubword = ""
                    Else
                        ' Continue building the subword
                        currentSubword = combinedSubword
                    End If
                ElseIf i = tokenValue.Length - 1 Then
                    ' Reached the end of the token
                    subwordUnits.Add(currentSubword)
                End If
            Next

            ' Add the subword units as BitWord tokens to the vocabulary
            For Each subwordUnit In subwordUnits
                Dim subwordToken As New Token(TokenType.BitWord, subwordUnit)
                Tokens.Add(subwordToken)

                ' Update the vocabulary
                UpdateVocabulary(subwordUnit)

            Next
            Return Tokens
        End Function

        ''' <summary>
        ''' Produces a <centralResource> Vocabulary Object</centralResource>
        ''' </summary>
        ''' <param name="Corpus"></param>
        ''' <param name="TokenizeType">Type of tokenization (char, sent , word)</param>
        ''' <returns></returns>
        Public Function TokenizeCorpus(ByRef Corpus As List(Of String), Optional TokenizeType As TokenizationOption = TokenizationOption.Word) As List(Of String)
            Dim Cleaned As New List(Of String)
            Dim Tokenize As New Tokenizer(TokenizeType)
            centralResource.Add("0. Raw Corpus", Corpus)


            Dim UncleanCorpusVocab = Tokenizer.TokenizeInput(Corpus, TokenizeType)



            centralResource.Add("1. Tokenized Corpus", UncleanCorpusVocab)

            For Each item In UncleanCorpusVocab
                If Cleaned.Contains(item) = False Then
                    Cleaned.Add(item)
                    UpdateVocabulary(item)
                End If
            Next
            centralResource.Add("2. Corpus Vocabulary -", Cleaned)

            'Display updated Vocab
            centralResource.Add("1-Tokenizer Vocabulary", Vocabulary)


            'Optimize Tokens
            Dim Subwords As New List(Of String)
            For Each item In Cleaned
                Subwords.AddRange(TokenizeToSubword(item))
            Next
            Dim CleanSubWords As New List(Of String)
            For Each nitem In Subwords
                If CleanSubWords.Contains(nitem) = False Then
                    If nitem.Length > 1 Then

                        CleanSubWords.Add(nitem)
                    End If

                End If
            Next
            For Each item In CleanSubWords
                UpdateVocabulary(item)
            Next
            centralResource.Add("3. Corpus Frequent Sub-words", CleanSubWords)
            'Get Frequent Ngrams
            Select Case TokenizeType
                Case TokenizationOption.Character
                    centralResource.Add("Corpus Character Bigrams", FindFrequentCharacterBigrams(Corpus))
                    centralResource.Add("Corpus Character Trigrams", FindFrequentCharacterTrigrams(Corpus))

                Case TokenizationOption.Word
                    centralResource.Add("Corpus Word Bigrams", FindFrequentWordBigrams(Corpus))
                    centralResource.Add("Corpus Word Trigrams", FindFrequentWordTrigrams(Corpus))


                Case TokenizationOption.Sentence
                    centralResource.Add("Corpus Sentence Bigrams", FindFrequentSentenceBigrams(Corpus))
                    centralResource.Add("Corpus Sentence Trigrams", FindFrequentSentenceTrigrams(Corpus))

            End Select

            'Train
            TrainVocabulary(VocabularyWithFrequency, 10)
            centralResource.Add("-Trained Tokenizer Vocabulary", Vocabulary)







            Return UncleanCorpusVocab
        End Function

        ''' <summary>
        ''' Pure Tokenizer With SubWordTokenization (for Unknown Words)
        ''' </summary>
        ''' <param name="input"></param>
        ''' <returns></returns>
        Public Function TokenizeGeneral(input As String) As List(Of String)
            Dim tokens As List(Of String) = input.Split(" ").ToList()

            ' Tokenize using the learned vocabulary
            Dim tokenizedText As List(Of String) = New List(Of String)

            For Each token As String In tokens
                If VocabularyWithFrequency.ContainsKey(token) Then
                    tokenizedText.Add(token)
                Else
                    ' If a token is not present in the vocabulary, split it into subword units
                    Dim subwordUnits As List(Of String) = TokenizeToSubword(token)
                    'Return Unknown As Subunits
                    tokenizedText.AddRange(subwordUnits)
                    'Token Is Updated in Vocabulary
                    UpdateVocabulary(token)
                End If
            Next

            Return tokenizedText
        End Function

        ''' <summary>
        ''' Returns Ngrams From Input
        ''' </summary>
        ''' <param name="Input"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function TokenizeToNGrams(ByRef Input As String, n As Integer) As List(Of Token)
            Dim tokens As List(Of Token) = Tokenize(Input)
            Dim nGrams As New List(Of Token)
            'PreProcess
            Input = NormalizeInput(Input)
            Select Case ModelType
                Case TokenizerType.Character Or TokenizerType.Ngram
                    For i As Integer = 0 To tokens.Count - n Step n
                        Dim nGramValue As String = ""

                        For j As Integer = 0 To n - 1
                            nGramValue &= tokens(i + j).Value
                        Next

                        Dim tokenType As String = GetTokenType(nGramValue)
                        nGrams.Add(New Token(tokenType, nGramValue))
                    Next
                Case TokenizerType.WordGram Or TokenizerType.Word
                    For i As Integer = 0 To tokens.Count - n Step n
                        Dim nGramValue As String = ""

                        For j As Integer = 0 To n - 1
                            nGramValue &= " " & tokens(i + j).Value
                        Next

                        Dim tokenType As String = GetTokenType(nGramValue)
                        nGrams.Add(New Token(tokenType, nGramValue))
                    Next
                Case TokenizerType.SentenceGram Or TokenizerType.Sentence
                    For i As Integer = 0 To tokens.Count - n Step n
                        Dim nGramValue As String = ""

                        For j As Integer = 0 To n - 1
                            nGramValue &= " " & tokens(i + j).Value
                        Next

                        Dim tokenType As String = GetTokenType(nGramValue)
                        nGrams.Add(New Token(tokenType, nGramValue))
                    Next
            End Select

            For Each item In nGrams
                UpdateVocabulary(item.Value)



            Next
            Return nGrams
        End Function

        Public Function TokenizeToSubTokens(word As String) As List(Of String)
            Dim tokens As New List(Of String)

            While word.Length > 0
                Dim prefixFound = False
                For i = word.Length To 1 Step -1
                    Dim subword = word.Substring(0, i)
                    If Vocabulary.Contains(subword) Then
                        tokens.Add(subword)
                        word = word.Substring(i)
                        prefixFound = True
                        Exit For
                    End If
                Next

                If Not prefixFound Then
                    tokens.Add("[UNK]")
                    word = String.Empty
                End If
            End While

            Return tokens
        End Function


        Public Sub Train(tokens As List(Of String), ByRef numMergeOperations As Integer)

            ' Initialize the vocabulary with individual tokens
            For Each token As String In tokens
                UpdateVocabulary(token)
            Next

            For mergeCount As Integer = 0 To numMergeOperations - 1
                ' Compute the frequency of adjacent token pairs
                Dim pairFrequencies As Dictionary(Of String, Integer) = ComputePairFrequencies()

                ' Find the most frequent pair
                Dim mostFrequentPair As KeyValuePair(Of String, Integer) = pairFrequencies.OrderByDescending(Function(x) x.Value).First()

                ' Merge the most frequent pair into a new unit
                Dim newUnit As String = mostFrequentPair.Key
                If VocabularyWithFrequency.ContainsKey(newUnit) = False Then


                    VocabularyWithFrequency.Add(newUnit, mostFrequentPair.Value)
                Else

                    Dim x = VocabularyWithFrequency(newUnit)
                    x += 1
                    VocabularyWithFrequency.Remove(newUnit)
                    VocabularyWithFrequency.Add(newUnit, x)
                End If
                ' Update the training data by replacing the merged pair with the new unit
                tokens = ReplaceMergedPair(tokens, newUnit)

                ' Update the vocabulary with the new unit and its frequency
                VocabularyWithFrequency(newUnit) = pairFrequencies.Values.Sum()
            Next

            TrainVocabulary(VocabularyWithFrequency)

        End Sub


        Private Shared Function AddSuffix(ByRef Str As String, ByVal Suffix As String) As String
            Return Str & Suffix
        End Function

        Private Shared Function ContainsEncapsulated(ByRef Userinput As String) As Boolean
            Dim Start = False
            Dim Ending = False
            ContainsEncapsulated = False
            For Each item In EncapuslationPunctuationStart
                If Userinput.Contains(item) = True Then Start = True
            Next
            For Each item In EncapuslationPunctuationEnd
                If Userinput.Contains(item) = True Then Ending = True
            Next
            If Start And Ending = True Then
                ContainsEncapsulated = True
            End If
        End Function


        Private Shared Function TokenizeToCharacter(sentence As String) As List(Of String)
            Dim characters As Char() = sentence.ToCharArray()
            Dim characterTokens As New List(Of String)

            For Each character In characters
                characterTokens.Add(character.ToString())
            Next

            Return characterTokens
        End Function

        Private Shared Function TokenizeToSentence(ByVal input As String) As List(Of String)
            Dim sentences As String() = input.Split("."c)

            Return sentences.ToList
        End Function

        Private Shared Function TokenizeToSubword(sentence As String) As List(Of String)
            Dim frequency As New Dictionary(Of String, Integer)
            If sentence.Length > 3 Then
                ' Initialize the frequency dictionary with character counts
                For Each word In sentence.Split()
                    For Each chara In word
                        Dim ch As String = chara.ToString()
                        If frequency.ContainsKey(ch) Then
                            frequency(ch) += 1
                        Else
                            frequency.Add(ch, 1)
                        End If
                    Next
                Next

                ' Merge the most frequent character pairs until a desired vocabulary size is reached
                Dim vocab As New List(Of String)(frequency.Keys)
                Dim vocabSize As Integer = 10 ' Set the desired vocabulary size
                While vocab.Count < vocabSize
                    Dim pairs As New Dictionary(Of String, Integer)

                    ' Calculate frequencies of character pairs
                    For Each word In sentence.Split()
                        Dim chars() As String = word.Select(Function(c) c.ToString()).ToArray()

                        For i As Integer = 0 To chars.Length - 2
                            Dim pair As String = chars(i) & chars(i + 1)
                            If pairs.ContainsKey(pair) Then
                                pairs(pair) += 1
                            Else
                                pairs.Add(pair, 1)
                            End If
                        Next
                    Next

                    ' Find the most frequent pair
                    Dim mostFrequentPair As String = pairs.OrderByDescending(Function(p) p.Value).First().Key

                    ' Merge the most frequent pair in the vocabulary
                    vocab.Add(mostFrequentPair)

                    ' Update the sentence by merging the most frequent pair
                    sentence = sentence.Replace(mostFrequentPair, mostFrequentPair.Replace(" ", ""))

                    ' Update the frequency dictionary with the merged pair
                    frequency(mostFrequentPair) = pairs(mostFrequentPair)

                    ' Remove the merged pair from the frequency dictionary
                    pairs.Remove(mostFrequentPair)
                End While
                Return vocab.Distinct.ToList
            Else
                Return New List(Of String)
            End If


        End Function

        Private Shared Function TokenizeToWords(sentence As String) As List(Of String)
            Dim wordPattern As String = "\b\w+\b"
            Dim wordTokens As New List(Of String)

            Dim matches As MatchCollection = Regex.Matches(sentence, wordPattern)

            For Each match As Match In matches
                wordTokens.Add(match.Value)
            Next

            Return wordTokens
        End Function


        Private Function ComputePairFrequencies() As Dictionary(Of String, Integer)
            Dim pairFrequencies As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()

            For Each token As String In VocabularyWithFrequency.Keys
                Dim tokenChars As List(Of Char) = token.ToList()

                For i As Integer = 0 To tokenChars.Count - 2
                    Dim pair As String = tokenChars(i) & tokenChars(i + 1)

                    If Not pairFrequencies.ContainsKey(pair) Then
                        pairFrequencies.Add(pair, VocabularyWithFrequency(token))
                    Else
                        pairFrequencies(pair) += VocabularyWithFrequency(token)
                    End If
                Next
            Next

            Return pairFrequencies
        End Function

        Private Function CreateNgrams(ByVal input As String, ByVal n As Integer) As List(Of Token)
            input = NormalizeInput(input)
            Dim tokens As New List(Of Token)
            Dim words As String() = input.Split(" "c)

            For i As Integer = 0 To words.Length - n
                Dim ngramValue As String = String.Join(" ", words.Skip(i).Take(n))
                Dim startPosition As Integer = words.Take(i).Sum(Function(w) w.Length) + i * 2 ' Account for the spaces between words
                Dim endPosition As Integer = startPosition + ngramValue.Length - 1
                Dim token As New Token(GetTokenType(ngramValue), ngramValue, startPosition, endPosition)
                tokens.Add(token)
            Next
            For Each item In tokens

                UpdateVocabulary(item.Value)


            Next
            Return tokens
        End Function


        Private Function GetModelType(ByVal tokenValue As String) As String
            Select Case ModelType
                Case TokenizerType.Word Or TokenizerType.WordGram
                    Return "Word"
                Case TokenizerType.Sentence Or TokenizerType.SentenceGram
                    Return "Sentence"
                Case TokenizerType.Character Or TokenizerType.Ngram
                    Return "Character"
                Case Else
                    Throw New Exception("Invalid token type.")
            End Select
        End Function

        Private Function GetRegexPattern(Optional TokenType As TokenType = Nothing) As String
            If TokenType = Nothing Then

                Dim entityRegExTypes As New Dictionary(Of String, String)() From {
        {"PERSON", "([A-Z][a-zA-Z]+)"},
        {"ORGANIZATION", "([A-Z][\w&]+(?:\s[A-Z][\w&]+)*)"},
        {"LOCATION", "([A-Z][\w\s&-]+)"},
        {"DATE", "(\d{1,2}\/\d{1,2}\/\d{2,4})"},
        {"TIME", "(\d{1,2}:\d{2}(?::\d{2})?)"},
        {"MONEY", "(\$\d+(?:\.\d{2})?)"},
        {"PERCENT", "(\d+(?:\.\d+)?%)"}
       }

                Select Case ModelType
                    Case TokenizerType.Word Or TokenizerType.WordGram
                        Return "(\b\w+\b|[^\w\s])"
                    Case TokenizerType.Sentence Or TokenizerType.SentenceGram
                        Return ".*?[.!?]"
                    Case TokenizerType.Ngram Or TokenizerType.Character
                        Return "(?s)."
                    Case Else
                        Throw New Exception("Invalid Tokenizer model type.")
                End Select
            Else
                Select Case TokenType
                    Case TokenType.Word
                        Return "(\b\w+\b|[^\w\s])"
                    Case TokenType.Sentence
                        Return ".*?[.!?]"
                    Case TokenType.Character
                        Return "(?s)."
                    Case TokenType.Number
                        Return "\d+"
                    Case TokenType.Punctuation
                        Return "[-+*/=(),.;:{}[\]<>]"
                    Case TokenType.whitespace
                        Return "\s+"


                    Case Else
                        Throw New Exception("Invalid token type.")
                End Select
            End If

            Return Nothing

        End Function

        ''' <summary>
        ''' Normalizes the input string by converting it to lowercase and removing punctuation and extra whitespace.
        ''' </summary>
        ''' <param name="input">The input string.</param>
        ''' <returns>The normalized input string.</returns>
        Private Function NormalizeInput(input As String) As String
            ' Convert to lowercase
            Dim normalizedInput As String = input.ToLower()

            input = SpacePunctuation(input)

            If StopWordRemovalEnabled Then
                input = RemoveStopWords(input)
            End If
            ' Remove extra whitespace
            normalizedInput = Regex.Replace(normalizedInput, "\s+", " ")

            Return normalizedInput
        End Function

        Private Function RemoveStopWords(ByRef txt As String) As String
            For Each item In Me.StopWords
                txt = txt.Replace(item, "")
            Next
            Return txt
        End Function

        Private Function RemoveStopWords(ByVal tokens As List(Of Token)) As List(Of Token)
            Return tokens.Where(Function(token) Not StopWords.Contains(token.Value)).ToList()
        End Function

        Private Function ReplaceMergedPair(tokens As List(Of String), newUnit As String) As List(Of String)
            Dim mergedTokens As List(Of String) = New List(Of String)

            For Each token As String In tokens
                Dim replacedToken As String = token.Replace(newUnit, " " & newUnit & " ")
                mergedTokens.AddRange(replacedToken.Split(" ").ToList())
            Next

            Return mergedTokens
        End Function

        Private Function SplitIntoSubwords(token As String, ByRef ngramLength As Integer) As List(Of String)
            Dim subwordUnits As List(Of String) = New List(Of String)

            For i As Integer = 0 To token.Length - ngramLength
                Dim subword As String = token.Substring(i, ngramLength)
                subwordUnits.Add(subword)
            Next

            Return subwordUnits
        End Function

        ''' <summary>
        ''' Centralized update Point(ADD TOKEN TO VOCABULARY)
        ''' </summary>
        ''' <param name="Item"></param>
        Private Sub UpdateVocabulary(ByRef Item As String)
            If VocabularyWithFrequency.ContainsKey(Item) = False Then
                VocabularyWithFrequency.Add(Item, 1)
            Else
                Dim x = VocabularyWithFrequency(Item)
                x += 1
                VocabularyWithFrequency.Remove(Item)
                VocabularyWithFrequency.Add(Item, x)
            End If

        End Sub

        ''' <summary>
        ''' Add Tokens to Vocabulary
        ''' </summary>
        ''' <param name="tokens"></param>
        Private Sub UpdateVocabulary(ByVal tokens As List(Of Token))
            For Each token In tokens

                UpdateVocabulary(token.Value)
            Next

        End Sub

        Public Structure Token
            ''' <summary>
            ''' Initializes a new instance of the Token structure.
            ''' </summary>
            ''' <param name="type">The type of the token.</param>
            ''' <param name="value">The string value of the token.</param>
            Public Sub New(ByVal type As String, ByVal value As String)
                Me.Type = type
                Me.Value = value
            End Sub

            Public Sub New(ByVal type As TokenType, ByVal value As String, ByVal startPosition As Integer, ByVal endPosition As Integer)
                Me.Type = type
                Me.Value = value
                Me.StartPosition = startPosition
                Me.EndPosition = endPosition
            End Sub

            Public Property EndPosition As Integer
            Public Property StartPosition As Integer
            Public Property Type As TokenType
            Public Property Value As String
        End Structure



    End Class

End Namespace

using System.Collections.Generic;
using System.Runtime;

using LangAnalyzer.Morphology;

namespace LangAnalyzer.Postagger
{
    internal sealed class MorphoAmbiguityPreProcessor
    {
        private const MorphoAttributeEnum MorphoAttributeAllCases =
            MorphoAttributeEnum.Nominative |
            MorphoAttributeEnum.Genitive |
            MorphoAttributeEnum.Dative |
            MorphoAttributeEnum.Accusative |
            MorphoAttributeEnum.Instrumental |
            MorphoAttributeEnum.Prepositional |
            MorphoAttributeEnum.Locative |
            MorphoAttributeEnum.Anycase;
        private const MorphoAttributeEnum MorphoAttributeAllNumber =
            MorphoAttributeEnum.Singular |
            MorphoAttributeEnum.Plural;
        private const MorphoAttributeEnum MorphoAttributeAllGender =
            MorphoAttributeEnum.Feminine |
            MorphoAttributeEnum.Masculine |
            MorphoAttributeEnum.Neuter |
            MorphoAttributeEnum.General;

        private static bool IsCaseAnycase(MorphoAttributeEnum morphoAttribute)
        {
            return (morphoAttribute & MorphoAttributeEnum.Anycase) == MorphoAttributeEnum.Anycase;
        }

        private static bool IsGenderGeneral(MorphoAttributeEnum morphoAttribute)
        {
            return (morphoAttribute & MorphoAttributeEnum.General) == MorphoAttributeEnum.General;
        }

        private readonly List<MorphoAmbiguityTuple> _mats_0;
        private readonly List<MorphoAmbiguityTuple> _mats_1;

        private readonly HashSet<MorphoAmbiguityTuple> _mats_0_stepII;
        private readonly HashSet<MorphoAmbiguityTuple> _mats_1_stepII;
        private readonly HashSet<MorphoAmbiguityTuple> _mats_2_stepII;

        public MorphoAmbiguityPreProcessor()
        {
            _mats_0 = new List<MorphoAmbiguityTuple>();
            _mats_1 = new List<MorphoAmbiguityTuple>();

            _mats_0_stepII = new HashSet<MorphoAmbiguityTuple>();
            _mats_1_stepII = new HashSet<MorphoAmbiguityTuple>();
            _mats_2_stepII = new HashSet<MorphoAmbiguityTuple>();
        }

        #region description
        /*
        [-Case_1-]
        ��������� ����:
        1.
         1.1
        - Preposition + PossessivePronoun + Noun
        - Preposition + AdjectivePronoun  + Noun
        - Preposition + Adjective         + Noun
����������.�a) �����������:
- Preposition + PossessivePronoun + Noun
- Preposition + AdjectivePronoun  + Noun
���� ������ ��������� �� ����������� �� ������, �� ������ ��������� ����������� � �������, � ������ ������������ �������.
�������:�� ��� ������;
b) ��� �������:
- Preposition + Adjective + Noun
� ������� (��� ��������) ���������� ���� �����Anycase (���General) ���Unknown�(�����������), �� �������� ��� �������� �� �������� ������������ ������� � ������� (��� ������� �� ������ ��������������).
�������: � ��������� ��������; � ����������������� �����; �� ������� ����; � ������������ �������; �� ����������� Ferrari         
        
         1.2
        - Preposition + Noun
        - Preposition + Pronoun
        - Preposition + Numeral
        - Preposition + PossessivePronoun
        - Preposition + Adjective
        - Preposition + AdjectivePronoun
            - ������ ������, ������ ����������� �� ������ (case). 
              �.�������, ����� ������ � �������� ���-�� ��������, ������� ������ ��, � ������� ������ ���������.
����������. a) �����������:
- Preposition + Noun
- Preposition + Pronoun
- Preposition + Numeral
-�Preposition + Adjective
������ ������ ��������� ����� �����Anycase (���General) ���Unknown�(�����������), �� �������� ��� �������� ����� ������� ����������.
�������: � ��������; ��iPhone; �� 23;         

        [-Case_2-]
        ��������� ����:
        2.
        - PossessivePronoun + Noun 
        - Adjective         + Noun
        - Adjective         + Adjective
        - AdjectivePronoun  + Noun
        - AdjectivePronoun  + Pronoun
        - AdjectivePronoun  + AdjectivePronoun
        - AdjectivePronoun  + Adjective
        - Numeral           + Noun
            - ������ ������, ������ ����������� �� ������ (case), ����� (number) � ���� (gender). 
            �.�������, ����� ������ � �������� ���-�� ��������, ������� ������ ��, 
            1) � ������� ������ ��������� (case); 
            2) ���� �������� ����� �����, �� ��������� ����� (number); 
            3) ���� �������� ����� �����, �� ��������� ��� (gender). 
            ������������� ��������� �������� �� ��� ���, ���� �� ���� �� ��������� �� ���������.
����������.�a) �����������:
- Adjective + Noun
- Adjective + Adjective
- Numeral   + Noun
� ������� (��� �������) ���������� ���� �����Anycase ���Unknown�(�����������), �� �������� ��� �������� �� ����� ������� (��� �������������� �������).
�������: ��������� ��������; �����Ferrari; ��������� ������������         
        */

        /*
        - Preposition + PossessivePronoun + Noun
        �� ��� ������, �� �������� � ������ ����������  �����
        ���� � �� �������� ��� ��������   ��� ����� ���
        
        - Preposition + AdjectivePronoun + Noun
        ����� ��� ����	���� ����� ���� �� �� ��������� ����
        � ��� ������ ��������� �����-�� �����
        
        - Preposition + Adjective + Noun
        ������ ���������� ����� ����� �� �� ��������� ��������



        - Preposition + Noun
        ���� ������ � �������

        - Preposition + PossessivePronoun
        ���� ������ � ����� �������        

        - Preposition + Pronoun
        ���� ������ � ����

        - Preposition + Numeral
        ���� ������ � ������ �� ������
        :
        - Adjective + Noun
        ���� ����� �������� ���

        - AdjectivePronoun + Noun
        ��� ���� ����� � �������

        - AdjectivePronoun + Pronoun
        ��� �� �������� ������ 

        - PossessivePronoun + Adjective
        ���� ������ ������ ������� �����

        - Adjective + Adjective
        ���������� �������� �������� ��������� �����

        - Numeral + Noun
        ������ ���� ��� �� ����������       
        */
        #endregion

        public void Run(List<WordMorphoAmbiguity> wordMorphoAmbiguities)
        {
            var len = wordMorphoAmbiguities.Count;
            if (len < 2)
            {
                return;
            }

            var wma_0 = wordMorphoAmbiguities[0];
            var wma_1 = wordMorphoAmbiguities[1];
            if (len == 2)
            {
                Run(wma_0, wma_1);

                return;
            }

            var wma_2 = wordMorphoAmbiguities[2];
            for (var wasModify = false; ; wasModify = false)
            {
                for (var i = 3; ; i++)
                {
                    switch (wma_0.word.posTaggerOutputType)
                    {
                        //Preposition: {+ PossessivePronoun, + AdjectivePronoun, + Adjective} + Noun
                        //Preposition: {+ Noun, + Pronoun, + Numeral, + PossessivePronoun, + Adjective, + AdjectivePronoun}
                        case PosTaggerOutputType.Preposition:
                            var Case_1__1_wasMatch = false;

                            if (wma_2.word.posTaggerOutputType == PosTaggerOutputType.Noun)
                            {
                                switch (wma_1.word.posTaggerOutputType)
                                {
                                    case PosTaggerOutputType.PossessivePronoun:
                                    case PosTaggerOutputType.AdjectivePronoun:
                                    case PosTaggerOutputType.Adjective:
                                        Case_1__1_wasMatch = Case_1__1(wma_0, wma_1, wma_2);
                                        break;
                                }
                            }

                            if (!Case_1__1_wasMatch)
                            {
                                switch (wma_1.word.posTaggerOutputType)
                                {
                                    case PosTaggerOutputType.Noun:
                                    case PosTaggerOutputType.Pronoun:
                                    case PosTaggerOutputType.Numeral:
                                    case PosTaggerOutputType.PossessivePronoun:
                                    case PosTaggerOutputType.Adjective:
                                    case PosTaggerOutputType.AdjectivePronoun:
                                        Case_1__2(wma_0, wma_1);
                                        break;
                                }
                            }
                            break;

                        //PossessivePronoun + Noun 
                        case PosTaggerOutputType.PossessivePronoun:
                            if (wma_1.word.posTaggerOutputType == PosTaggerOutputType.Pronoun)
                            {
                                wasModify |= Case_2(wma_0, wma_1);
                            }
                            break;

                        //Adjective: {+ Noun, + Adjective}
                        case PosTaggerOutputType.Adjective:
                            switch (wma_1.word.posTaggerOutputType)
                            {
                                case PosTaggerOutputType.Noun:
                                case PosTaggerOutputType.Adjective:
                                    wasModify |= Case_2(wma_0, wma_1);
                                    break;
                            }
                            break;

                        //AdjectivePronoun: {+ Noun, + Pronoun, + AdjectivePronoun, + Adjective}
                        case PosTaggerOutputType.AdjectivePronoun:
                            switch (wma_1.word.posTaggerOutputType)
                            {
                                case PosTaggerOutputType.Noun:
                                case PosTaggerOutputType.Pronoun:
                                case PosTaggerOutputType.AdjectivePronoun:
                                case PosTaggerOutputType.Adjective:
                                    wasModify |= Case_2(wma_0, wma_1);
                                    break;
                            }
                            break;

                        //Numeral + Noun
                        case PosTaggerOutputType.Numeral:
                            if (wma_1.word.posTaggerOutputType == PosTaggerOutputType.Noun)
                            {
                                wasModify |= Case_2(wma_0, wma_1);
                            }
                            break;
                    }

                    if (len <= i)
                    {
                        break;
                    }

                    wma_0 = wma_1;
                    wma_1 = wma_2;
                    wma_2 = wordMorphoAmbiguities[i];
                }

                if (!wasModify)
                {
                    break;
                }
            }

            Run(wma_1, wma_2);
        }

        private void Run(WordMorphoAmbiguity wma_0, WordMorphoAmbiguity wma_1)
        {
            switch (wma_0.word.posTaggerOutputType)
            {
                //Preposition: {+ Noun, + Pronoun, + Numeral, + PossessivePronoun, + Adjective, + AdjectivePronoun}
                case PosTaggerOutputType.Preposition:
                    switch (wma_1.word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Noun:
                        case PosTaggerOutputType.Pronoun:
                        case PosTaggerOutputType.Numeral:
                        case PosTaggerOutputType.PossessivePronoun:
                        case PosTaggerOutputType.Adjective:
                        case PosTaggerOutputType.AdjectivePronoun:
                            Case_1__2(wma_0, wma_1);
                            break;
                    }
                    break;

                //PossessivePronoun + Noun 
                case PosTaggerOutputType.PossessivePronoun:
                    if (wma_1.word.posTaggerOutputType == PosTaggerOutputType.Pronoun)
                    {
                        Case_2(wma_0, wma_1);
                    }
                    break;

                //Adjective: {+ Noun, + Adjective}
                case PosTaggerOutputType.Adjective:
                    switch (wma_1.word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Noun:
                        case PosTaggerOutputType.Adjective:
                            Case_2(wma_0, wma_1);
                            break;
                    }
                    break;

                //AdjectivePronoun: {+ Noun, + Pronoun, + AdjectivePronoun, + Adjective}
                case PosTaggerOutputType.AdjectivePronoun:
                    switch (wma_1.word.posTaggerOutputType)
                    {
                        case PosTaggerOutputType.Noun:
                        case PosTaggerOutputType.Pronoun:
                        case PosTaggerOutputType.AdjectivePronoun:
                        case PosTaggerOutputType.Adjective:
                            Case_2(wma_0, wma_1);
                            break;
                    }
                    break;

                //Numeral + Noun
                case PosTaggerOutputType.Numeral:
                    if (wma_1.word.posTaggerOutputType == PosTaggerOutputType.Noun)
                    {
                        Case_2(wma_0, wma_1);
                    }
                    break;
            }
        }

        private bool Case_1__1(WordMorphoAmbiguity wma_0, WordMorphoAmbiguity wma_1, WordMorphoAmbiguity wma_2)
        {
            var len_0 = wma_0.morphoAmbiguityTuples.Count;
            var len_1 = wma_1.morphoAmbiguityTuples.Count;

            if (len_0 == 1)
            {
                if (1 < len_1)
                {
                    var ma_case_0 = (wma_0.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_1; j++)
                        {
                            var mat_1 = wma_1.morphoAmbiguityTuples[j];
                            var ma_1 = mat_1.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_1 & MorphoAttributeAllCases))
                            {
                                _mats_1.Add(mat_1);
                            }
                        }

                        if (_mats_1.Count != 0)
                        {
                            var wasModify = (_mats_1.Count < len_1);
                            if (wasModify)
                            {
                                wasModify = Case_1__1_stepII_1(_mats_1, wma_2);
                                if (wasModify)
                                {
                                    wma_1.morphoAmbiguityTuples.Clear();
                                    wma_1.morphoAmbiguityTuples.AddRange(_mats_1);
                                }
                            }
                            _mats_1.Clear();

                            return wasModify;
                        }
                    }
                }
            }
            else if (len_1 == 1)
            {
                var ma_case_1 = (wma_1.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                if (!IsCaseAnycase(ma_case_1))
                {
                    for (var i = 0; i < len_0; i++)
                    {
                        var mat_0 = wma_0.morphoAmbiguityTuples[i];
                        var ma_0 = mat_0.wordFormMorphology.MorphoAttribute;
                        if (ma_case_1 == (ma_0 & MorphoAttributeAllCases))
                        {
                            _mats_0.Add(mat_0);
                        }
                    }

                    if (_mats_0.Count != 0)
                    {
                        var wasModify = (_mats_0.Count < len_0);
                        if (wasModify)
                        {
                            wasModify = Case_1__1_stepII_1(_mats_0, wma_2);
                            if (wasModify)
                            {
                                wma_0.morphoAmbiguityTuples.Clear();
                                wma_0.morphoAmbiguityTuples.AddRange(_mats_0);
                            }
                        }
                        _mats_0.Clear();

                        return wasModify;
                    }
                }
            }
            else
            {
                for (var i = 0; i < len_0; i++)
                {
                    var mat_0 = wma_0.morphoAmbiguityTuples[i];
                    var ma_case_0 = (mat_0.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_1; j++)
                        {
                            var mat_1 = wma_1.morphoAmbiguityTuples[j];
                            var ma_1 = mat_1.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_1 & MorphoAttributeAllCases))
                            {
                                _mats_0.AddIfNotExists(mat_0);
                                _mats_1.AddIfNotExists(mat_1);
                            }
                        }
                    }
                }

                if (_mats_0.Count != 0)
                {
                    var wasModify = ((_mats_0.Count < len_0) || (_mats_1.Count < len_1));
                    if (wasModify)
                    {
                        wasModify = Case_1__1_stepII_2(wma_2);
                        if (wasModify)
                        {
                            wma_0.morphoAmbiguityTuples.Clear();
                            wma_0.morphoAmbiguityTuples.AddRange(_mats_0);

                            wma_1.morphoAmbiguityTuples.Clear();
                            wma_1.morphoAmbiguityTuples.AddRange(_mats_1);
                        }
                    }
                    _mats_0.Clear();
                    _mats_1.Clear();

                    return wasModify;
                }
            }

            return false;
        }
        private bool Case_1__1_stepII_1(List<MorphoAmbiguityTuple> mats, WordMorphoAmbiguity wma_2)
        {
            var len_0 = mats.Count;
            var len_2 = wma_2.morphoAmbiguityTuples.Count;

            if (len_0 == 1)
            {
                if (1 < len_2)
                {
                    var ma_case_0 = (mats[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_2; j++)
                        {
                            var mat_2 = wma_2.morphoAmbiguityTuples[j];
                            var ma_2 = mat_2.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_2 & MorphoAttributeAllCases))
                            {
                                _mats_2_stepII.Add(mat_2);
                            }
                        }

                        if (_mats_2_stepII.Count != 0)
                        {
                            var wasModify = (_mats_2_stepII.Count < len_2);
                            if (wasModify)
                            {
                                wma_2.morphoAmbiguityTuples.Clear();
                                wma_2.morphoAmbiguityTuples.AddRange(_mats_2_stepII);
                            }
                            _mats_2_stepII.Clear();

                            return wasModify;
                        }
                    }
                }
            }
            else if (len_2 == 1)
            {
                var ma_case_2 = (wma_2.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                if (!IsCaseAnycase(ma_case_2))
                {
                    for (var i = 0; i < len_0; i++)
                    {
                        var mat_0 = mats[i];
                        var ma_0 = mat_0.wordFormMorphology.MorphoAttribute;
                        if (ma_case_2 == (ma_0 & MorphoAttributeAllCases))
                        {
                            _mats_0_stepII.Add(mat_0);
                        }
                    }

                    if (_mats_0_stepII.Count != 0)
                    {
                        var wasModify = (_mats_0_stepII.Count < len_0);
                        if (wasModify)
                        {
                            mats.Clear();
                            mats.AddRange(_mats_0_stepII);
                        }
                        _mats_0_stepII.Clear();

                        return wasModify;
                    }
                }
            }
            else
            {
                for (var i = 0; i < len_0; i++)
                {
                    var mat_0 = mats[i];
                    var ma_case_0 = (mat_0.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_2; j++)
                        {
                            var mat_2 = wma_2.morphoAmbiguityTuples[j];
                            var ma_2 = mat_2.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_2 & MorphoAttributeAllCases))
                            {
                                _mats_0_stepII.Add(mat_0);
                                _mats_2_stepII.Add(mat_2);
                            }
                        }
                    }
                }

                if (_mats_0_stepII.Count != 0)
                {
                    var wasModify = ((_mats_0_stepII.Count < len_0) || (_mats_2_stepII.Count < len_2));
                    if (wasModify)
                    {
                        mats.Clear();
                        mats.AddRange(_mats_0_stepII);

                        wma_2.morphoAmbiguityTuples.Clear();
                        wma_2.morphoAmbiguityTuples.AddRange(_mats_2_stepII);
                    }
                    _mats_0_stepII.Clear();
                    _mats_2_stepII.Clear();

                    return wasModify;
                }
            }

            return false;
        }
        private bool Case_1__1_stepII_2(WordMorphoAmbiguity wma_2)
        {
            System.Diagnostics.Debug.Assert(_mats_0.Count == _mats_1.Count, "_Mats_0.Count != _Mats_1.Count");

            var len_0 = _mats_0.Count;
            var len_2 = wma_2.morphoAmbiguityTuples.Count;

            if (len_0 == 1)
            {
                if (1 < len_2)
                {
                    var ma_case_0 = (_mats_0[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_2; j++)
                        {
                            var mat_2 = wma_2.morphoAmbiguityTuples[j];
                            var ma_2 = mat_2.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_2 & MorphoAttributeAllCases))
                            {
                                _mats_2_stepII.Add(mat_2);
                            }
                        }

                        if (_mats_2_stepII.Count != 0)
                        {
                            var wasModify = (_mats_2_stepII.Count < len_2);
                            if (wasModify)
                            {
                                wma_2.morphoAmbiguityTuples.Clear();
                                wma_2.morphoAmbiguityTuples.AddRange(_mats_2_stepII);
                            }
                            _mats_2_stepII.Clear();

                            return wasModify;
                        }
                    }
                }
            }
            else if (len_2 == 1)
            {
                var ma_case_2 = (wma_2.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                if (!IsCaseAnycase(ma_case_2))
                {
                    for (var i = 0; i < len_0; i++)
                    {
                        var mat_0 = _mats_0[i];
                        var ma_0 = mat_0.wordFormMorphology.MorphoAttribute;
                        if (ma_case_2 == (ma_0 & MorphoAttributeAllCases))
                        {
                            _mats_0_stepII.Add(mat_0);
                            var mat_1 = _mats_1[i];
                            _mats_1_stepII.Add(mat_1);
                        }
                    }

                    if (_mats_0_stepII.Count != 0)
                    {
                        var wasModify = (_mats_0_stepII.Count < len_0);
                        if (wasModify)
                        {
                            _mats_0.Clear();
                            _mats_0.AddRange(_mats_0_stepII);

                            _mats_1.Clear();
                            _mats_1.AddRange(_mats_1_stepII);
                        }
                        _mats_0_stepII.Clear();
                        _mats_1_stepII.Clear();

                        return wasModify;
                    }
                }
            }
            else
            {
                for (var i = 0; i < len_0; i++)
                {
                    var mat_0 = _mats_0[i];
                    var ma_case_0 = (mat_0.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_2; j++)
                        {
                            var mat_2 = wma_2.morphoAmbiguityTuples[j];
                            var ma_2 = mat_2.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_2 & MorphoAttributeAllCases))
                            {
                                _mats_0_stepII.Add(mat_0);
                                var mat_1 = _mats_1[i];
                                _mats_1_stepII.Add(mat_1);
                                _mats_2_stepII.Add(mat_2);
                            }
                        }
                    }
                }

                if (_mats_0_stepII.Count != 0)
                {
                    var wasModify = ((_mats_0_stepII.Count < len_0) || (_mats_2_stepII.Count < len_2));
                    if (wasModify)
                    {
                        _mats_0.Clear();
                        _mats_0.AddRange(_mats_0_stepII);

                        _mats_1.Clear();
                        _mats_1.AddRange(_mats_1_stepII);

                        wma_2.morphoAmbiguityTuples.Clear();
                        wma_2.morphoAmbiguityTuples.AddRange(_mats_2_stepII);
                    }
                    _mats_0_stepII.Clear();
                    _mats_1_stepII.Clear();
                    _mats_2_stepII.Clear();

                    return wasModify;
                }
            }

            return false;
        }

        private void Case_1__2(WordMorphoAmbiguity wma_0, WordMorphoAmbiguity wma_1)
        {
            var len_0 = wma_0.morphoAmbiguityTuples.Count;
            var len_1 = wma_1.morphoAmbiguityTuples.Count;

            if (len_0 == 1)
            {
                if (1 < len_1)
                {
                    var ma_case_0 = (wma_0.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_1; j++)
                        {
                            var mat_1 = wma_1.morphoAmbiguityTuples[j];
                            var ma_1 = mat_1.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_1 & MorphoAttributeAllCases))
                            {
                                _mats_1.Add(mat_1);
                            }
                        }

                        if (_mats_1.Count != 0)
                        {
                            var wasModify = (_mats_1.Count < len_1);
                            if (wasModify)
                            {
                                wma_1.morphoAmbiguityTuples.Clear();
                                wma_1.morphoAmbiguityTuples.AddRange(_mats_1);
                            }
                            _mats_1.Clear();
                        }
                    }
                }
            }
            else if (len_1 == 1)
            {
                var ma_case_1 = (wma_1.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                if (!IsCaseAnycase(ma_case_1))
                {
                    for (var i = 0; i < len_0; i++)
                    {
                        var mat_0 = wma_0.morphoAmbiguityTuples[i];
                        var ma_0 = mat_0.wordFormMorphology.MorphoAttribute;
                        if (ma_case_1 == (ma_0 & MorphoAttributeAllCases))
                        {
                            _mats_0.Add(mat_0);
                        }
                    }

                    if (_mats_0.Count != 0)
                    {
                        var wasModify = (_mats_0.Count < len_0);
                        if (wasModify)
                        {
                            wma_0.morphoAmbiguityTuples.Clear();
                            wma_0.morphoAmbiguityTuples.AddRange(_mats_0);
                        }
                        _mats_0.Clear();
                    }
                }
            }
            else
            {
                for (var i = 0; i < len_0; i++)
                {
                    var mat_0 = wma_0.morphoAmbiguityTuples[i];
                    var ma_case_0 = (mat_0.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_1; j++)
                        {
                            var mat_1 = wma_1.morphoAmbiguityTuples[j];
                            var ma_1 = mat_1.wordFormMorphology.MorphoAttribute;
                            if (ma_case_0 == (ma_1 & MorphoAttributeAllCases))
                            {
                                _mats_0.AddIfNotExists(mat_0);
                                _mats_1.AddIfNotExists(mat_1);
                            }
                        }
                    }
                }

                if (_mats_0.Count != 0)
                {
                    var wasModify = ((_mats_0.Count < len_0) || (_mats_1.Count < len_1));
                    if (wasModify)
                    {
                        wma_0.morphoAmbiguityTuples.Clear();
                        wma_0.morphoAmbiguityTuples.AddRange(_mats_0);

                        wma_1.morphoAmbiguityTuples.Clear();
                        wma_1.morphoAmbiguityTuples.AddRange(_mats_1);
                    }
                    _mats_0.Clear();
                    _mats_1.Clear();
                }
            }
        }

        private bool Case_2(WordMorphoAmbiguity wma_0, WordMorphoAmbiguity wma_1)
        {
            var len_0 = wma_0.morphoAmbiguityTuples.Count;
            var len_1 = wma_1.morphoAmbiguityTuples.Count;

            if (len_0 == 1)
            {
                if (1 < len_1)
                {
                    var ma_0 = wma_0.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute;
                    var ma_case_0 = (ma_0 & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_1; j++)
                        {
                            var mat_1 = wma_1.morphoAmbiguityTuples[j];
                            if (ma_case_0 == (mat_1.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases))
                            {
                                _mats_1.Add(mat_1);
                            }
                        }

                        if (_mats_1.Count != 0)
                        {
                            if (Case_2_TryFilterByMask(MorphoAttributeAllNumber, ma_0, _mats_1))
                            {
                                if (!IsGenderGeneral(ma_0))
                                {
                                    Case_2_TryFilterByMask(MorphoAttributeAllGender, ma_0, _mats_1);
                                }
                            }

                            var wasModify = (_mats_1.Count < wma_1.morphoAmbiguityTuples.Count);
                            if (wasModify)
                            {
                                wma_1.morphoAmbiguityTuples.Clear();
                                wma_1.morphoAmbiguityTuples.AddRange(_mats_1);
                            }
                            _mats_1.Clear();
                            return wasModify;
                        }
                    }
                }
            }
            else if (len_1 == 1)
            {
                var ma_1 = wma_1.morphoAmbiguityTuples[0].wordFormMorphology.MorphoAttribute;
                var ma_case_1 = (ma_1 & MorphoAttributeAllCases);
                if (!IsCaseAnycase(ma_case_1))
                {
                    for (var i = 0; i < len_0; i++)
                    {
                        var mat_0 = wma_0.morphoAmbiguityTuples[i];
                        if (ma_case_1 == (mat_0.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases))
                        {
                            _mats_0.Add(mat_0);
                        }
                    }

                    if (_mats_0.Count != 0)
                    {
                        if (Case_2_TryFilterByMask(MorphoAttributeAllNumber, ma_1, _mats_0))
                        {
                            if (!IsGenderGeneral(ma_1))
                            {
                                Case_2_TryFilterByMask(MorphoAttributeAllGender, ma_1, _mats_0);
                            }
                        }

                        var wasModify = (_mats_0.Count < wma_0.morphoAmbiguityTuples.Count);
                        if (wasModify)
                        {
                            wma_0.morphoAmbiguityTuples.Clear();
                            wma_0.morphoAmbiguityTuples.AddRange(_mats_0);
                        }
                        _mats_0.Clear();
                        return wasModify;
                    }
                }
            }
            else
            {
                for (var i = 0; i < len_0; i++)
                {
                    var mat_0 = wma_0.morphoAmbiguityTuples[i];
                    var ma_0 = mat_0.wordFormMorphology.MorphoAttribute;
                    var ma_case_0 = (ma_0 & MorphoAttributeAllCases);
                    if (!IsCaseAnycase(ma_case_0))
                    {
                        for (var j = 0; j < len_1; j++)
                        {
                            var mat_1 = wma_1.morphoAmbiguityTuples[j];
                            if (ma_case_0 == (mat_1.wordFormMorphology.MorphoAttribute & MorphoAttributeAllCases))
                            {
                                _mats_0.AddIfNotExists(mat_0);
                                _mats_1.AddIfNotExists(mat_1);
                            }
                        }
                    }
                }

                if (_mats_0.Count != 0)
                {
                    if (Case_2_TryFilterByNumber(_mats_0, _mats_1))
                    {
                        Case_2_TryFilterByGender(_mats_0, _mats_1);
                    }

                    var wasModify = (_mats_0.Count < wma_0.morphoAmbiguityTuples.Count) ||
                                     (_mats_1.Count < wma_1.morphoAmbiguityTuples.Count);
                    if (wasModify)
                    {
                        wma_0.morphoAmbiguityTuples.Clear();
                        wma_0.morphoAmbiguityTuples.AddRange(_mats_0);

                        wma_1.morphoAmbiguityTuples.Clear();
                        wma_1.morphoAmbiguityTuples.AddRange(_mats_1);
                    }
                    _mats_1.Clear();
                    _mats_0.Clear();
                    return wasModify;
                }
            }

            return false;
        }
        private static bool Case_2_TryFilterByMask(MorphoAttributeEnum mask, MorphoAttributeEnum ma, List<MorphoAmbiguityTuple> mats)
        {
            var len = mats.Count - 1;
            if (0 < len)
            {
                ma &= mask;
                for (var i = len; 0 <= i; i--)
                {
                    var mat = mats[i];
                    if (ma == (mat.wordFormMorphology.MorphoAttribute & mask))
                    {
                        for (i = len; 0 <= i; i--)
                        {
                            mat = mats[i];
                            if (ma != (mat.wordFormMorphology.MorphoAttribute & mask))
                            {
                                mats.RemoveAt(i);
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool Case_2_TryFilterByNumber(List<MorphoAmbiguityTuple> mats_0, List<MorphoAmbiguityTuple> mats_1)
        {
            var len = mats_0.Count;
            if (1 < len || 1 < mats_1.Count)
            {
                var wasModify = false;
                for (var i = 0; i < len; i++)
                {
                    wasModify |= Case_2_TryFilterByMask(MorphoAttributeAllNumber, mats_0[i].wordFormMorphology.MorphoAttribute, mats_1);
                }
                return wasModify;
            }
            return false;
        }
        private static bool Case_2_TryFilterByGender(List<MorphoAmbiguityTuple> mats_0, List<MorphoAmbiguityTuple> mats_1)
        {
            var len = mats_0.Count;
            if (1 < len || 1 < mats_1.Count)
            {
                var wasModify = false;
                for (var i = 0; i < len; i++)
                {
                    var ma = mats_0[i].wordFormMorphology.MorphoAttribute;
                    if (!IsGenderGeneral(ma))
                    {
                        wasModify |= Case_2_TryFilterByMask(MorphoAttributeAllGender, ma, mats_1);
                    }
                }
                return wasModify;
            }
            return false;
        }
    }

    internal static class MorphoAmbiguityPreProcessorExt
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public static void AddIfNotExists(this List<MorphoAmbiguityTuple> mats, MorphoAmbiguityTuple mat)
        {
            for (int i = 0, len = mats.Count; i < len; i++)
            {
                if (Equals(mats[i], mat))
                {
                    return;
                }
            }

            mats.Add(mat);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private static bool Equals(MorphoAmbiguityTuple x, MorphoAmbiguityTuple y)
        {
            if (!WordFormMorphology.Equals(ref x.wordFormMorphology, ref y.wordFormMorphology))
                return false;

            if (string.CompareOrdinal(x.word.valueUpper, y.word.valueUpper) != 0)
                return false;

            return x.punctuationType == y.punctuationType;
        }
    }
}

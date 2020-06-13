namespace ME.ECS.Collections.StackArray {

    using System;
    using System.Runtime.CompilerServices;
    
    #pragma warning disable
    public struct A10<T>where T:struct{public T p0,p1,p2,p3,p4,p5,p6,p7,p8,p9;}public struct A20<T>where T:struct{public T p0,p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19;}public struct A30<T>where T:struct{public T p0,p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29;}public struct A40<T>where T:struct{public T p0,p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29,p30,p31,p32,p33,p34,p35,p36,p37,p38,p39;}public struct A50<T>where T:struct{public T p0,p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29,p30,p31,p32,p33,p34,p35,p36,p37,p38,p39,p40,p41,p42,p43,p44,p45,p46,p47,p48,p49;}public struct A1000<T>where T:struct{public T p0,p1,p2,p3,p4,p5,p6,p7,p8,p9,p10,p11,p12,p13,p14,p15,p16,p17,p18,p19,p20,p21,p22,p23,p24,p25,p26,p27,p28,p29,p30,p31,p32,p33,p34,p35,p36,p37,p38,p39,p40,p41,p42,p43,p44,p45,p46,p47,p48,p49,p50,p51,p52,p53,p54,p55,p56,p57,p58,p59,p60,p61,p62,p63,p64,p65,p66,p67,p68,p69,p70,p71,p72,p73,p74,p75,p76,p77,p78,p79,p80,p81,p82,p83,p84,p85,p86,p87,p88,p89,p90,p91,p92,p93,p94,p95,p96,p97,p98,p99,p100,p101,p102,p103,p104,p105,p106,p107,p108,p109,p110,p111,p112,p113,p114,p115,p116,p117,p118,p119,p120,p121,p122,p123,p124,p125,p126,p127,p128,p129,p130,p131,p132,p133,p134,p135,p136,p137,p138,p139,p140,p141,p142,p143,p144,p145,p146,p147,p148,p149,p150,p151,p152,p153,p154,p155,p156,p157,p158,p159,p160,p161,p162,p163,p164,p165,p166,p167,p168,p169,p170,p171,p172,p173,p174,p175,p176,p177,p178,p179,p180,p181,p182,p183,p184,p185,p186,p187,p188,p189,p190,p191,p192,p193,p194,p195,p196,p197,p198,p199,p200,p201,p202,p203,p204,p205,p206,p207,p208,p209,p210,p211,p212,p213,p214,p215,p216,p217,p218,p219,p220,p221,p222,p223,p224,p225,p226,p227,p228,p229,p230,p231,p232,p233,p234,p235,p236,p237,p238,p239,p240,p241,p242,p243,p244,p245,p246,p247,p248,p249,p250,p251,p252,p253,p254,p255,p256,p257,p258,p259,p260,p261,p262,p263,p264,p265,p266,p267,p268,p269,p270,p271,p272,p273,p274,p275,p276,p277,p278,p279,p280,p281,p282,p283,p284,p285,p286,p287,p288,p289,p290,p291,p292,p293,p294,p295,p296,p297,p298,p299,p300,p301,p302,p303,p304,p305,p306,p307,p308,p309,p310,p311,p312,p313,p314,p315,p316,p317,p318,p319,p320,p321,p322,p323,p324,p325,p326,p327,p328,p329,p330,p331,p332,p333,p334,p335,p336,p337,p338,p339,p340,p341,p342,p343,p344,p345,p346,p347,p348,p349,p350,p351,p352,p353,p354,p355,p356,p357,p358,p359,p360,p361,p362,p363,p364,p365,p366,p367,p368,p369,p370,p371,p372,p373,p374,p375,p376,p377,p378,p379,p380,p381,p382,p383,p384,p385,p386,p387,p388,p389,p390,p391,p392,p393,p394,p395,p396,p397,p398,p399,p400,p401,p402,p403,p404,p405,p406,p407,p408,p409,p410,p411,p412,p413,p414,p415,p416,p417,p418,p419,p420,p421,p422,p423,p424,p425,p426,p427,p428,p429,p430,p431,p432,p433,p434,p435,p436,p437,p438,p439,p440,p441,p442,p443,p444,p445,p446,p447,p448,p449,p450,p451,p452,p453,p454,p455,p456,p457,p458,p459,p460,p461,p462,p463,p464,p465,p466,p467,p468,p469,p470,p471,p472,p473,p474,p475,p476,p477,p478,p479,p480,p481,p482,p483,p484,p485,p486,p487,p488,p489,p490,p491,p492,p493,p494,p495,p496,p497,p498,p499,p500,p501,p502,p503,p504,p505,p506,p507,p508,p509,p510,p511,p512,p513,p514,p515,p516,p517,p518,p519,p520,p521,p522,p523,p524,p525,p526,p527,p528,p529,p530,p531,p532,p533,p534,p535,p536,p537,p538,p539,p540,p541,p542,p543,p544,p545,p546,p547,p548,p549,p550,p551,p552,p553,p554,p555,p556,p557,p558,p559,p560,p561,p562,p563,p564,p565,p566,p567,p568,p569,p570,p571,p572,p573,p574,p575,p576,p577,p578,p579,p580,p581,p582,p583,p584,p585,p586,p587,p588,p589,p590,p591,p592,p593,p594,p595,p596,p597,p598,p599,p600,p601,p602,p603,p604,p605,p606,p607,p608,p609,p610,p611,p612,p613,p614,p615,p616,p617,p618,p619,p620,p621,p622,p623,p624,p625,p626,p627,p628,p629,p630,p631,p632,p633,p634,p635,p636,p637,p638,p639,p640,p641,p642,p643,p644,p645,p646,p647,p648,p649,p650,p651,p652,p653,p654,p655,p656,p657,p658,p659,p660,p661,p662,p663,p664,p665,p666,p667,p668,p669,p670,p671,p672,p673,p674,p675,p676,p677,p678,p679,p680,p681,p682,p683,p684,p685,p686,p687,p688,p689,p690,p691,p692,p693,p694,p695,p696,p697,p698,p699,p700,p701,p702,p703,p704,p705,p706,p707,p708,p709,p710,p711,p712,p713,p714,p715,p716,p717,p718,p719,p720,p721,p722,p723,p724,p725,p726,p727,p728,p729,p730,p731,p732,p733,p734,p735,p736,p737,p738,p739,p740,p741,p742,p743,p744,p745,p746,p747,p748,p749,p750,p751,p752,p753,p754,p755,p756,p757,p758,p759,p760,p761,p762,p763,p764,p765,p766,p767,p768,p769,p770,p771,p772,p773,p774,p775,p776,p777,p778,p779,p780,p781,p782,p783,p784,p785,p786,p787,p788,p789,p790,p791,p792,p793,p794,p795,p796,p797,p798,p799,p800,p801,p802,p803,p804,p805,p806,p807,p808,p809,p810,p811,p812,p813,p814,p815,p816,p817,p818,p819,p820,p821,p822,p823,p824,p825,p826,p827,p828,p829,p830,p831,p832,p833,p834,p835,p836,p837,p838,p839,p840,p841,p842,p843,p844,p845,p846,p847,p848,p849,p850,p851,p852,p853,p854,p855,p856,p857,p858,p859,p860,p861,p862,p863,p864,p865,p866,p867,p868,p869,p870,p871,p872,p873,p874,p875,p876,p877,p878,p879,p880,p881,p882,p883,p884,p885,p886,p887,p888,p889,p890,p891,p892,p893,p894,p895,p896,p897,p898,p899,p900,p901,p902,p903,p904,p905,p906,p907,p908,p909,p910,p911,p912,p913,p914,p915,p916,p917,p918,p919,p920,p921,p922,p923,p924,p925,p926,p927,p928,p929,p930,p931,p932,p933,p934,p935,p936,p937,p938,p939,p940,p941,p942,p943,p944,p945,p946,p947,p948,p949,p950,p951,p952,p953,p954,p955,p956,p957,p958,p959,p960,p961,p962,p963,p964,p965,p966,p967,p968,p969,p970,p971,p972,p973,p974,p975,p976,p977,p978,p979,p980,p981,p982,p983,p984,p985,p986,p987,p988,p989,p990,p991,p992,p993,p994,p995,p996,p997,p998,p999;}
    #pragma warning restore

}

namespace ME.ECS.Collections {
    
    #pragma warning disable
    
    using ME.ECS.Collections.StackArray;
    using Unity.Collections.LowLevel.Unsafe;
    using System.Runtime.CompilerServices;
    
    public interface IStackArray {

        int Length { get; }
        object this[int index] { get; set; }

    }

    internal static class H {

        [MethodImpl(256)]
        public static unsafe T R<T, TT>(ref TT t, int i) where TT : struct {
            
            return UnsafeUtility.ReadArrayElement<T>(UnsafeUtility.AddressOf(ref t), i);
            
        }

        [MethodImpl(256)]
        public static unsafe void W<T, TT>(ref TT t, int i, T value) where TT : struct {
            
            UnsafeUtility.WriteArrayElement(UnsafeUtility.AddressOf(ref t), i, value);
            
        }

    }

    [System.Serializable]
public struct StackArray10<T> : IStackArray where T : struct {

    public const int MAX_LENGTH = 10;
    public A10<T> arr;
    
    private int length;
    
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public StackArray10(int length) {
        
        this.arr = new A10<T>();
        this.length = (length > StackArray10<T>.MAX_LENGTH ? StackArray10<T>.MAX_LENGTH : length);

    }

    public int Length {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            return this.length;
        }
    }

    object IStackArray.this[int index] {
        get {
            return this[index];
        }
        set {
            this[index] = (T)value;
        }
    }

    public T this[int index] {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            switch (index) {
                case 0: return this.arr.p0;case 1: return this.arr.p1;case 2: return this.arr.p2;case 3: return this.arr.p3;case 4: return this.arr.p4;case 5: return this.arr.p5;case 6: return this.arr.p6;case 7: return this.arr.p7;case 8: return this.arr.p8;case 9: return this.arr.p9;
            }
            return default;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        set {
            switch (index) {
                case 0: this.arr.p0 = value; return;case 1: this.arr.p1 = value; return;case 2: this.arr.p2 = value; return;case 3: this.arr.p3 = value; return;case 4: this.arr.p4 = value; return;case 5: this.arr.p5 = value; return;case 6: this.arr.p6 = value; return;case 7: this.arr.p7 = value; return;case 8: this.arr.p8 = value; return;case 9: this.arr.p9 = value; return;
            }
        }
    }

}
[System.Serializable]
public struct StackArray20<T> : IStackArray where T : struct {

    public const int MAX_LENGTH = 20;
    public A20<T> arr;
    
    private int length;
    
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public StackArray20(int length) {
        
        this.arr = new A20<T>();
        this.length = (length > StackArray20<T>.MAX_LENGTH ? StackArray20<T>.MAX_LENGTH : length);

    }

    public int Length {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            return this.length;
        }
    }

    object IStackArray.this[int index] {
        get {
            return this[index];
        }
        set {
            this[index] = (T)value;
        }
    }

    public T this[int index] {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            switch (index) {
                case 0: return this.arr.p0;case 1: return this.arr.p1;case 2: return this.arr.p2;case 3: return this.arr.p3;case 4: return this.arr.p4;case 5: return this.arr.p5;case 6: return this.arr.p6;case 7: return this.arr.p7;case 8: return this.arr.p8;case 9: return this.arr.p9;case 10: return this.arr.p10;case 11: return this.arr.p11;case 12: return this.arr.p12;case 13: return this.arr.p13;case 14: return this.arr.p14;case 15: return this.arr.p15;case 16: return this.arr.p16;case 17: return this.arr.p17;case 18: return this.arr.p18;case 19: return this.arr.p19;
            }
            return default;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        set {
            switch (index) {
                case 0: this.arr.p0 = value; return;case 1: this.arr.p1 = value; return;case 2: this.arr.p2 = value; return;case 3: this.arr.p3 = value; return;case 4: this.arr.p4 = value; return;case 5: this.arr.p5 = value; return;case 6: this.arr.p6 = value; return;case 7: this.arr.p7 = value; return;case 8: this.arr.p8 = value; return;case 9: this.arr.p9 = value; return;case 10: this.arr.p10 = value; return;case 11: this.arr.p11 = value; return;case 12: this.arr.p12 = value; return;case 13: this.arr.p13 = value; return;case 14: this.arr.p14 = value; return;case 15: this.arr.p15 = value; return;case 16: this.arr.p16 = value; return;case 17: this.arr.p17 = value; return;case 18: this.arr.p18 = value; return;case 19: this.arr.p19 = value; return;
            }
        }
    }

}
[System.Serializable]
public struct StackArray30<T> : IStackArray where T : struct {

    public const int MAX_LENGTH = 30;
    public A30<T> arr;
    
    private int length;
    
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public StackArray30(int length) {
        
        this.arr = new A30<T>();
        this.length = (length > StackArray30<T>.MAX_LENGTH ? StackArray30<T>.MAX_LENGTH : length);

    }

    public int Length {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            return this.length;
        }
    }

    object IStackArray.this[int index] {
        get {
            return this[index];
        }
        set {
            this[index] = (T)value;
        }
    }

    public T this[int index] {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            switch (index) {
                case 0: return this.arr.p0;case 1: return this.arr.p1;case 2: return this.arr.p2;case 3: return this.arr.p3;case 4: return this.arr.p4;case 5: return this.arr.p5;case 6: return this.arr.p6;case 7: return this.arr.p7;case 8: return this.arr.p8;case 9: return this.arr.p9;case 10: return this.arr.p10;case 11: return this.arr.p11;case 12: return this.arr.p12;case 13: return this.arr.p13;case 14: return this.arr.p14;case 15: return this.arr.p15;case 16: return this.arr.p16;case 17: return this.arr.p17;case 18: return this.arr.p18;case 19: return this.arr.p19;case 20: return this.arr.p20;case 21: return this.arr.p21;case 22: return this.arr.p22;case 23: return this.arr.p23;case 24: return this.arr.p24;case 25: return this.arr.p25;case 26: return this.arr.p26;case 27: return this.arr.p27;case 28: return this.arr.p28;case 29: return this.arr.p29;
            }
            return default;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        set {
            switch (index) {
                case 0: this.arr.p0 = value; return;case 1: this.arr.p1 = value; return;case 2: this.arr.p2 = value; return;case 3: this.arr.p3 = value; return;case 4: this.arr.p4 = value; return;case 5: this.arr.p5 = value; return;case 6: this.arr.p6 = value; return;case 7: this.arr.p7 = value; return;case 8: this.arr.p8 = value; return;case 9: this.arr.p9 = value; return;case 10: this.arr.p10 = value; return;case 11: this.arr.p11 = value; return;case 12: this.arr.p12 = value; return;case 13: this.arr.p13 = value; return;case 14: this.arr.p14 = value; return;case 15: this.arr.p15 = value; return;case 16: this.arr.p16 = value; return;case 17: this.arr.p17 = value; return;case 18: this.arr.p18 = value; return;case 19: this.arr.p19 = value; return;case 20: this.arr.p20 = value; return;case 21: this.arr.p21 = value; return;case 22: this.arr.p22 = value; return;case 23: this.arr.p23 = value; return;case 24: this.arr.p24 = value; return;case 25: this.arr.p25 = value; return;case 26: this.arr.p26 = value; return;case 27: this.arr.p27 = value; return;case 28: this.arr.p28 = value; return;case 29: this.arr.p29 = value; return;
            }
        }
    }

}
[System.Serializable]
public struct StackArray40<T> : IStackArray where T : struct {

    public const int MAX_LENGTH = 40;
    public A40<T> arr;
    
    private int length;
    
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public StackArray40(int length) {
        
        this.arr = new A40<T>();
        this.length = (length > StackArray40<T>.MAX_LENGTH ? StackArray40<T>.MAX_LENGTH : length);

    }

    public int Length {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            return this.length;
        }
    }

    object IStackArray.this[int index] {
        get {
            return this[index];
        }
        set {
            this[index] = (T)value;
        }
    }

    public T this[int index] {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            switch (index) {
                case 0: return this.arr.p0;case 1: return this.arr.p1;case 2: return this.arr.p2;case 3: return this.arr.p3;case 4: return this.arr.p4;case 5: return this.arr.p5;case 6: return this.arr.p6;case 7: return this.arr.p7;case 8: return this.arr.p8;case 9: return this.arr.p9;case 10: return this.arr.p10;case 11: return this.arr.p11;case 12: return this.arr.p12;case 13: return this.arr.p13;case 14: return this.arr.p14;case 15: return this.arr.p15;case 16: return this.arr.p16;case 17: return this.arr.p17;case 18: return this.arr.p18;case 19: return this.arr.p19;case 20: return this.arr.p20;case 21: return this.arr.p21;case 22: return this.arr.p22;case 23: return this.arr.p23;case 24: return this.arr.p24;case 25: return this.arr.p25;case 26: return this.arr.p26;case 27: return this.arr.p27;case 28: return this.arr.p28;case 29: return this.arr.p29;case 30: return this.arr.p30;case 31: return this.arr.p31;case 32: return this.arr.p32;case 33: return this.arr.p33;case 34: return this.arr.p34;case 35: return this.arr.p35;case 36: return this.arr.p36;case 37: return this.arr.p37;case 38: return this.arr.p38;case 39: return this.arr.p39;
            }
            return default;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        set {
            switch (index) {
                case 0: this.arr.p0 = value; return;case 1: this.arr.p1 = value; return;case 2: this.arr.p2 = value; return;case 3: this.arr.p3 = value; return;case 4: this.arr.p4 = value; return;case 5: this.arr.p5 = value; return;case 6: this.arr.p6 = value; return;case 7: this.arr.p7 = value; return;case 8: this.arr.p8 = value; return;case 9: this.arr.p9 = value; return;case 10: this.arr.p10 = value; return;case 11: this.arr.p11 = value; return;case 12: this.arr.p12 = value; return;case 13: this.arr.p13 = value; return;case 14: this.arr.p14 = value; return;case 15: this.arr.p15 = value; return;case 16: this.arr.p16 = value; return;case 17: this.arr.p17 = value; return;case 18: this.arr.p18 = value; return;case 19: this.arr.p19 = value; return;case 20: this.arr.p20 = value; return;case 21: this.arr.p21 = value; return;case 22: this.arr.p22 = value; return;case 23: this.arr.p23 = value; return;case 24: this.arr.p24 = value; return;case 25: this.arr.p25 = value; return;case 26: this.arr.p26 = value; return;case 27: this.arr.p27 = value; return;case 28: this.arr.p28 = value; return;case 29: this.arr.p29 = value; return;case 30: this.arr.p30 = value; return;case 31: this.arr.p31 = value; return;case 32: this.arr.p32 = value; return;case 33: this.arr.p33 = value; return;case 34: this.arr.p34 = value; return;case 35: this.arr.p35 = value; return;case 36: this.arr.p36 = value; return;case 37: this.arr.p37 = value; return;case 38: this.arr.p38 = value; return;case 39: this.arr.p39 = value; return;
            }
        }
    }

}
[System.Serializable]
public struct StackArray50<T> : IStackArray where T : struct {

    public const int MAX_LENGTH = 50;
    public A50<T> arr;
    
    private int length;
    
    [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
    public StackArray50(int length) {
        
        this.arr = new A50<T>();
        this.length = (length > StackArray50<T>.MAX_LENGTH ? StackArray50<T>.MAX_LENGTH : length);

    }

    public int Length {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            return this.length;
        }
    }

    object IStackArray.this[int index] {
        get {
            return this[index];
        }
        set {
            this[index] = (T)value;
        }
    }

    public T this[int index] {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        get {
            switch (index) {
                case 0: return this.arr.p0;case 1: return this.arr.p1;case 2: return this.arr.p2;case 3: return this.arr.p3;case 4: return this.arr.p4;case 5: return this.arr.p5;case 6: return this.arr.p6;case 7: return this.arr.p7;case 8: return this.arr.p8;case 9: return this.arr.p9;case 10: return this.arr.p10;case 11: return this.arr.p11;case 12: return this.arr.p12;case 13: return this.arr.p13;case 14: return this.arr.p14;case 15: return this.arr.p15;case 16: return this.arr.p16;case 17: return this.arr.p17;case 18: return this.arr.p18;case 19: return this.arr.p19;case 20: return this.arr.p20;case 21: return this.arr.p21;case 22: return this.arr.p22;case 23: return this.arr.p23;case 24: return this.arr.p24;case 25: return this.arr.p25;case 26: return this.arr.p26;case 27: return this.arr.p27;case 28: return this.arr.p28;case 29: return this.arr.p29;case 30: return this.arr.p30;case 31: return this.arr.p31;case 32: return this.arr.p32;case 33: return this.arr.p33;case 34: return this.arr.p34;case 35: return this.arr.p35;case 36: return this.arr.p36;case 37: return this.arr.p37;case 38: return this.arr.p38;case 39: return this.arr.p39;case 40: return this.arr.p40;case 41: return this.arr.p41;case 42: return this.arr.p42;case 43: return this.arr.p43;case 44: return this.arr.p44;case 45: return this.arr.p45;case 46: return this.arr.p46;case 47: return this.arr.p47;case 48: return this.arr.p48;case 49: return this.arr.p49;
            }
            return default;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        set {
            switch (index) {
                case 0: this.arr.p0 = value; return;case 1: this.arr.p1 = value; return;case 2: this.arr.p2 = value; return;case 3: this.arr.p3 = value; return;case 4: this.arr.p4 = value; return;case 5: this.arr.p5 = value; return;case 6: this.arr.p6 = value; return;case 7: this.arr.p7 = value; return;case 8: this.arr.p8 = value; return;case 9: this.arr.p9 = value; return;case 10: this.arr.p10 = value; return;case 11: this.arr.p11 = value; return;case 12: this.arr.p12 = value; return;case 13: this.arr.p13 = value; return;case 14: this.arr.p14 = value; return;case 15: this.arr.p15 = value; return;case 16: this.arr.p16 = value; return;case 17: this.arr.p17 = value; return;case 18: this.arr.p18 = value; return;case 19: this.arr.p19 = value; return;case 20: this.arr.p20 = value; return;case 21: this.arr.p21 = value; return;case 22: this.arr.p22 = value; return;case 23: this.arr.p23 = value; return;case 24: this.arr.p24 = value; return;case 25: this.arr.p25 = value; return;case 26: this.arr.p26 = value; return;case 27: this.arr.p27 = value; return;case 28: this.arr.p28 = value; return;case 29: this.arr.p29 = value; return;case 30: this.arr.p30 = value; return;case 31: this.arr.p31 = value; return;case 32: this.arr.p32 = value; return;case 33: this.arr.p33 = value; return;case 34: this.arr.p34 = value; return;case 35: this.arr.p35 = value; return;case 36: this.arr.p36 = value; return;case 37: this.arr.p37 = value; return;case 38: this.arr.p38 = value; return;case 39: this.arr.p39 = value; return;case 40: this.arr.p40 = value; return;case 41: this.arr.p41 = value; return;case 42: this.arr.p42 = value; return;case 43: this.arr.p43 = value; return;case 44: this.arr.p44 = value; return;case 45: this.arr.p45 = value; return;case 46: this.arr.p46 = value; return;case 47: this.arr.p47 = value; return;case 48: this.arr.p48 = value; return;case 49: this.arr.p49 = value; return;
            }
        }
    }

}


    [System.Serializable]
    public struct StackArray<T> : IStackArray where T : struct {

        public const int MAX_LENGTH = 1000;
        public A1000<T> arr;
        
        private int length;
        
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public StackArray(int length) {
            
            this.arr = new A1000<T>();
            this.length = (length > StackArray<T>.MAX_LENGTH ? StackArray<T>.MAX_LENGTH : length);

        }

        public int Length {
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            get {
                return this.length;
            }
        }

        object IStackArray.this[int index] {
            get {
                return this[index];
            }
            set {
                this[index] = (T)value;
            }
        }

        public T this[int index] {
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            get {
                return H.R<T, A1000<T>>(ref this.arr, index);
            }
            [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
            set {
                H.W(ref this.arr, index, value);
            }
        }

    }
    
    #pragma warning restore

}
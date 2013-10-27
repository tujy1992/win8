using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;

// 此文件定义的数据模型可充当在添加、移除或修改成员时
// 支持通知的强类型模型的代表性示例。所选
// 属性名称与标准项模板中的数据绑定一致。
//
// 应用程序可以使用此模型作为起始点并以它为基础构建，或完全放弃它并
// 替换为适合其需求的其他内容。

namespace App1.Data
{
    /// <summary>
    /// <see cref="SampleDataItem"/> 和 <see cref="SampleDataGroup"/> 的基类，
    /// 定义对两者通用的属性。
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : App1.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// 泛型项数据模型。
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// 泛型组数据模型。
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // 由于两个原因提供要从 GroupedItemsPage 绑定到的完整
            // 项集合的子集: GridView 不会虚拟化大型项集合，并且它
            // 可在浏览包含大量项的组时改进用户
            // 体验。
            //
            // 最多显示 12 项，因为无论显示 1、2、3、4 还是 6 行，
            // 它都生成填充网格列

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// 创建包含硬编码内容的组和项的集合。
    /// 
    /// SampleDataSource 用占位符数据而不是实时生产数据
    /// 初始化，因此在设计时和运行时均需提供示例数据。
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // 对于小型数据集可接受简单线性搜索
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // 对于小型数据集可接受简单线性搜索
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            String ITEM_CONTENT = String.Format("{0}\n\n{0}\n\n",
                        "");

            var group1 = new SampleDataGroup("Group-1",
                    "皮具",
                    "皮具",
                    "Assets/10.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            String ITEM_CONTENT11 = String.Format("{0}\n\n{1}\n\n",
                        "爱马仕（Hermès）始于1837年的法国，是一家忠于传统手工艺，不断追求创新的国际化企业。拥有箱包、丝巾、领带、男、女装和生活艺术品等十七类产品系列以及新近开发的家具、室内装饰品及墙纸系列。全球共25个分公司管理及分销来自四大范畴的产品：爱马仕鞍具及皮革、爱马仕香水、爱马仕钟表及爱马仕餐瓷。1996年进入中国，在北京开了第一家爱马仕（Hermes）专卖店。让所有的产品至精至美、无可挑剔，是Hermes的一贯宗旨。大多数产品都是手工精心制作的，无外乎有人称Hermes的产品为思想深邃、品位高尚、内涵丰富、工艺精湛的艺术品。这些Hermes精品，通过其散布于世界20多个国家和地区的200多家专卖店，融进快节奏的现代生活中，让世人重返传统优雅的怀抱。", "1996年进入中国，在北京开了第一家爱马仕（Hermes）专卖店。让所有的产品至精至美、无可挑剔，是Hermes的一贯宗旨。大多数产品都是手工精心制作的，无外乎有人称Hermes的产品为思想深邃、品位高尚、内涵丰富、工艺精湛的艺术品。这些Hermes精品，通过其散布于世界20多个国家和地区的200多家专卖店，融进快节奏的现代生活中，让世人重返传统优雅的怀抱。");
            group1.Items.Add(new SampleDataItem("Group-1-Item-1",
                    "爱马仕",
                    "Item Subtitle: 1",
                    "Assets/11.jpg",
                    "爱马仕（Hermès）是世界著名的奢侈品品牌，1837年由Thierry Hermès创立于法国巴黎，早年以制造高级马具起家，迄今已有170多年的悠久历史。爱马仕是一家忠于传统手工艺，不断追求创新的国际化企业，现已拥有箱包、丝巾领带、男、女装和生活艺术品等十七类产品系列。爱马仕的总店位于法国巴黎，分店遍布世界各地，1996年在北京开了中国第一家Hermes专卖店，“爱马仕”为大中华区统一中文译名。爱马仕一直秉承着超凡卓越、极至绚烂的设计理念，造就优雅之极的传统典范。",
                    ITEM_CONTENT11,
                    group1));

            String ITEM_CONTENT12 = String.Format("{0}\n\n{1}\n\n",
                       "路易威登的皮件精品可列为顶级名牌皮件商品之一。路易威登的精品并不只有皮件，它还有各式皮包、男女装、丝巾、钢笔手表等等。现在的设计总监是Marc Jacobs。", "全世界公认最顶级的品牌Louis Vuitton，任何国家的名媛绅士都是其爱用者，百年来一直以四瓣花跟路易威登的缩写组合，成为各时代潮流的领导者，亚洲国家尤其是日本更对Louis Vuitton疯狂的膜拜，近年来日本人则把品牌塑造成神话的境界，运用极具创意的包装行销手法，将Louis Vuitton推上与艺术合作的殿堂，路易威登历经时代的转变，不仅没有呈现老态还不断的登峰造极。");

            group1.Items.Add(new SampleDataItem("Group-1-Item-2",
                    "路易威登",
                    "Item Subtitle: 2",
                    "Assets/12.jpg",
                    "路易威登（LOUIS VUITTON）成立于1854年 ，读音是“luyis vitton”，简写为“LV”，中文译为“路易·威登”。 路易威登的第一代创始人是十九世纪一位法国巴黎专门为王室贵族打造旅行行李的技师路易威登Louis Vuitton ，他打造的制作的皮箱技术精良，在当时的巴黎名气非常响亮。进而使路易威登成为皮制旅行用品最精致的象征。",
                    ITEM_CONTENT12,
                    group1));
            String ITEM_CONTENT13 = String.Format("{0}\n\n",
                      "香奈儿品牌走高端路线，时尚简约、简单舒适、纯正风范、婉约大方、青春靓丽。“流行稍纵即逝，风格永存”，依然是品牌背后的指导力量；“华丽的反面不是贫穷，而是庸俗”，Chanel女士主导的香奈儿品牌最特别之处在于实用的华丽，她从生活周围撷取灵感，尤其是爱情，不像其他设计师要求别人配合他们的设计，Chanel品牌提供了具有解放意义的自由和选择，将服装设计从男性观点为主的潮流转变成表现女性美感的自主舞台，将女性本质的需求转化为香奈儿品牌的内涵。");

            group1.Items.Add(new SampleDataItem("Group-1-Item-3",
                    "香奈儿",
                    "Item Subtitle: 3",
                    "Assets/13.jpg",
                    "创始人Gabrielle Chanel香奈儿于1913年在法国巴黎创立香奈儿品牌。香奈儿的产品种类繁多，有服装、珠宝饰品及其配件、化妆品、护肤品、香水，每一种产品都闻名遐迩，特别是她的香水与时装。 香奈儿(CHANEL)是一个有80多年经历的著名品牌，香奈儿时装永远有着高雅、简洁、精美的风格，她善于突破传统，早20世纪40年代就成功地将“五花大绑”的女装推向简单、舒适，这也许就是最早的现代休闲服。",
                    ITEM_CONTENT13,
                    group1));
            
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("Group-2",
                    "名表",
                    "Group Subtitle: 2",
                    "Assets/20.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");

            String ITEM_CONTENT21 = String.Format("{0}\n\n",
                      "百达翡丽在钟表鉴赏家眼中无与伦比的声誉和地位，不仅源于他们精致完美的时计作品以及丰富的制表知识与技术。百达翡丽无可争议的非凡地位同样来自公司始终秉承1839年创立以来的卓越制表理念。这种精神早已融入品牌的十大价值，成为百达翡丽至尊品质的象征，同他们的时代杰作一样代代相传、流芳百世。");
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "百达翡丽",
                    "Item Subtitle: 1",
                    "Assets/21.jpg",
                    "百达翡丽是瑞士日内瓦仅存的家族独立经营制表商，历史悠久，工艺精湛，始终致力于设计、开发、精制并装配出世界上最完美的时计作品。",
                    ITEM_CONTENT21,
                    group2));
            String ITEM_CONTENT22 = String.Format("{0}\n\n",
                      "江诗丹顿在整个二十世纪推出了多款令人永世难忘的设计。从简约典雅的款式到精雕细琢的复杂时计，从日常佩戴的款式到名贵的钻石腕表，每一款均代表了瑞士高级钟表登峰造极的制表工艺，体现了江诗丹顿在世界钟表业界卓尔不群的地位，及其对技术和美学的追求。历经两个半世纪的风风雨雨，江诗丹顿至今依然是钟表业界最负盛名的品牌之一。");
       
            group2.Items.Add(new SampleDataItem("Group-2-Item-2",
                    "江诗丹顿",
                    "Item Subtitle: 2",
                    "Assets/22.jpg",
                    "江诗丹顿（Vacheron Constantin），世界最著名钟表品牌之一，1755年创立于瑞士日内瓦，为世界最古老最早的钟表制造厂，也是世界最著名的表厂之一。江诗丹顿传承了瑞士的传统制表精华，未曾间断，同时也创新了许多制表技术，对制表业有莫大的贡献。",
                    ITEM_CONTENT22,
                    group2));
            String ITEM_CONTENT23 = String.Format("{0}\n\n",
                     "1875年，两位钟表工匠——22岁的Jules-Louis Audemars和24岁的Edward-Auguste Piguet在瑞士侏罗山谷的布拉苏丝（Le Brassus）村庄共同创立了爱彼（Audemars Piguet）品牌。两人所坚持的理念，为品牌开创独树一帜的视野，并留下经典隽永的传世作品。直到今天，爱彼这个家族企业仍由后代子嗣一手打理，灵感巧思传承于血脉之中，对制表工艺的热情也丝毫未减。");
       
            group2.Items.Add(new SampleDataItem("Group-2-Item-3",
                    "Item Title: 3",
                    "Item Subtitle: 3",
                    "Assets/23.jpg",
                    "爱彼（Audemars Piguet），世界著名三大制表品牌之一。1875年创立于瑞士侏罗山谷的布拉苏丝（Le Brassus）村庄，是当地的一家独立家族企业，也是世界最著名的表厂之一。爱彼传承并发扬着瑞士传统制表精粹，始终秉承“驾驭常规，铸就创新”的品牌理念。每一款爱彼作品都将品牌百年的精湛技艺浓缩于其中，正是工匠大师们的不懈投入，方才呈现出品牌超凡卓越的心血结晶。时光的流动在爱彼的手中具体而微，出类拔萃。",
                    ITEM_CONTENT23,
                    group2));
            this.AllGroups.Add(group2);

            var group3 = new SampleDataGroup("Group-3",
                    "汽车",
                    "Group Subtitle: 3",
                    "Assets/30.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            String ITEM_CONTENT31 = String.Format("{0}\n\n",
                     "劳斯莱斯最与众不同之处，就在于它大量使用了手工劳动，在人工费相当高昂的英国，这必然会导致生产成本的居高不下，这也是劳斯莱斯价格惊人的原因之一。直到今天，劳斯莱斯的发动机还完全是用手工制造。更令人称奇的是，劳斯莱斯车头散热器的格栅完全是由熟练工人用手和眼来完成的，不用任何丈量的工具。而一台散热器需要一个工人一整天时间才能制造出来，然后还需要5个小时对它进行加工打磨。");
       
            group3.Items.Add(new SampleDataItem("Group-3-Item-1",
                    "劳斯莱斯",
                    "Item Subtitle: 1",
                    "Assets/31.jpg",
                    "劳斯莱斯（Rolls-Royce）是世界顶级豪华轿车厂商，1906年成立于英国，公司创始人为Frederick Henry Royce（亨利·莱斯）和Charles Stewart Rolls（查理·劳斯）。Rolls-Royce出产的轿车是顶级汽车的杰出代表，以豪华而享誉全球。除了制造汽车，劳斯莱斯还涉足飞机发动机制造领域，它也是世界上最优秀的发动机制造者，著名的波音客机用的就是劳斯莱斯的发动机。2003年劳斯莱斯汽车公司被宝马（BMW）接手。",
                    ITEM_CONTENT31,
                    group3));
            String ITEM_CONTENT32 = String.Format("{0}\n\n",
                    "迈巴赫是汽车历史上一个充满传奇色彩的品牌，巧夺天工的设计和无与伦比的精湛的制造技术使它在上个世纪初成为代表德国汽车工业最高水平的杰作。如果不是60年前的那场战争，迈巴赫恐怕早已成为与劳斯莱斯齐名的世界顶级豪华车。今天，德国人凭借自己的聪明才智和精湛的技艺使这一古老品牌迅速回复昔日的光辉，重新诠释了“Maybach（迈巴赫）”这一传奇品牌——个象征着完美和奢华的轿车。");
       
            group3.Items.Add(new SampleDataItem("Group-3-Item-2",
                    "迈巴赫",
                    "Item Subtitle: 2",
                    "Assets/32.jpg",
                    "迈巴赫（德文：Maybach）与迈巴赫引擎制造厂（德文：Maybach-Motorenbau GmbH）是一曾经在1921年到1940年间活跃于欧洲地区的德国超豪华汽车品牌与制造厂，车厂创始人卡尔·迈巴赫（Karl Maybach）的父亲威廉·迈巴赫（Wilhelm Maybach）曾担任戴姆勒发动机公司（今日戴姆勒·克莱斯勒集团前身）的首席技术总监，两厂渊源甚深。1997年戴姆勒·克莱斯勒集团在东京车展会场中展出一辆以Maybach为名的概念性超豪华四门轿车，正式让这个德国汽车品牌在销声匿迹多年后再次复活。但是由于市场业绩不佳，迈巴赫系列轿车将于2013年全面停产。",
                    ITEM_CONTENT32,
                    group3));
            String ITEM_CONTENT33 = String.Format("{0}\n\n",
                   "在汽车发展初级阶段的20世纪初期，沃尔特·欧文·本特利从赛车场上获得了灵感，萌生出制造一种注重性能且重量更轻的汽车的想法。这在人们对车的认识仅仅限于交通工具的时代，他的这种想法显得前卫而另类，也注定了宾利从诞生之始即非为“大众”而生的产物。 1912年宾利家族筹集资金引进法国的DFP跑车，并在伦敦的3 Hanover广场成立了Bentley & Bentley公司。W.O.对引进的DFP做了改进，将发动机改用铝活塞，提高了汽车的性能。");
       
            group3.Items.Add(new SampleDataItem("Group-3-Item-3",
                    "宾利",
                    "Item Subtitle: 3",
                    "Assets/33.jpg",
                    "宾利汽车公司（Bentley Motors Limited）是举世闻名的豪华汽车制造商，总部位于英国克鲁。1919年1月18日，公司由沃尔特·欧文·本特利创建。一战期间，宾利公司以生产航空发动机而闻名，战后，则开始设计制造汽车产品。1931年，宾利被劳斯莱斯收购，在1998年两者均被德国大众集团买下；同年8月，宝马以6800万美元的价格购得劳斯莱斯的商标使用权，双方关系逐渐弱化。在近百年的历史长河中，宾利品牌依然熠熠生辉，不断给世人呈现出尊贵、典雅与精工细做的高品质座驾。",
                    ITEM_CONTENT33,
                    group3));
           
            this.AllGroups.Add(group3);

            var group4 = new SampleDataGroup("Group-4",
                    "化妆品",
                    "Group Subtitle: 4",
                    "Assets/40.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            String ITEM_CONTENT41 = String.Format("{0}\n\n",
                  "La Prairie的特色是富含细胞精华。细胞精华主要是结合一百多种与人体肌肤所需营养成分相同的细胞营养滋养系统，及醣蛋白组合和植物精华，提供人体所需的营养成分，以活化及滋养人体细胞，使细胞改善并且强化本身肌肤的自然功能，如调节、保护、再生等能力。招牌的鱼子酱系列还运用了一般保养品罕见的鲟鱼子(Caviar)精华成分，La Prairie将这种传统用于鱼子酱内的成分，运用到保养品中，更增加该品牌保养品成分的独特性。");
       
            group4.Items.Add(new SampleDataItem("Group-4-Item-1",
                    "la prairie",
                    "Item Subtitle: 1",
                    "Assets/41.jpg",
                    "在瑞士的蒙特里斯有一座赫赫有名的laprairie疗养中心，LaPrairie护肤系 列研究所传承了其引以为豪的优良传统。在跨逾半个世纪的历史长河中，该疗养中心开创了以活细胞疗法抵抗衰老的先河，与来自世界各地的众多访客共享其独步全球的欣喜硕果。",
                    ITEM_CONTENT41,
                    group4));
            String ITEM_CONTENT42 = String.Format("{0}\n\n",
                 "HR赫莲娜 (Helena Rubinstein) 于1902年由赫莲娜·鲁宾斯坦女士 (Helena Rubinstein) 在澳大利亚创立，HR赫莲娜 (Helena Rubinstein) 被誉为“美容界的科学先驱”。HR赫莲娜 (Helena Rubinstein) 的明星产品包括Flame Look彩妆、Prodigy极致之美菁华系列护肤品等。1985年，HR赫莲娜 (Helena Rubinstein) 被欧莱雅集团 (L'Oréal) 收购，转型为今日具有高科技现代感的品牌，适合追求创新、尖端科技和前卫时尚的现代女性。");
       
            group4.Items.Add(new SampleDataItem("Group-4-Item-2",
                    "赫莲娜",
                    "Item Subtitle: 2",
                    "Assets/42.jpg",
                    "HR赫莲娜 (Helena Rubinstein) 是欧莱雅集团旗下的顶极奢华美容品牌，也是现代美容行业的奠基品牌之一。HR赫莲娜 (Helena Rubinstein) 严格遵守“严谨、科学、艺术、哲学、女性”的品牌内涵，同时不断创新，首创许多前卫大胆的作风，而成为美容品牌中的胜出者。",
                    ITEM_CONTENT42,
                    group4));
            String ITEM_CONTENT43 = String.Format("{0}\n\n",
                "凭借着对香水的天才敏感嗅觉、执着不懈的冒险精神，以及他立志让法国品牌在当时已被美国品牌垄断的全球化妆品市场占有一席之地的抱负，为世界化妆品历史写下美的一页。一支来自法国古堡的玫瑰，凭借着气质，在变幻莫测的女性心理间，在捉摸不定的时尚法则中，足足绽放了六十年。");
       
            group4.Items.Add(new SampleDataItem("Group-4-Item-3",
                    "兰蔻",
                    "Item Subtitle: 3",
                    "Assets/43.jpg",
                    "兰蔻诞生于1935年的法国，是由Armand Petitjean（阿曼达·珀蒂让）创办的品牌。作为全球知名的高端化妆品品牌，兰蔻涉足护肤、彩妆、香水等多个产品领域，主要面向教育程度、收入水平较高，年龄在25～40岁的成熟女性。",
                    ITEM_CONTENT43,
                    group4));
           
            this.AllGroups.Add(group4);

            var group5 = new SampleDataGroup("Group-5",
                    "手机",
                    "Group Subtitle: 5",
                    "Assets/50.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            String ITEM_CONTENT51 = String.Format("{0}\n\n",
                "所有Vertu手机均由Vertu工厂的巧手工匠精心制造。仅仅是Vertu Ascent的键盘，便由超过150个不同部件制成。每个按键均经过繁复工序，在高温下将不锈钢混合物和压力共同注入较大的模具，待到冷却后，每个按键的体积便会缩小14%，就形成了斜角键盘。然后，每个按键均会镶嵌在两个宝石轴承上，以便提升设计动感，同时增加触控稳定度与精确度，并为使用者带来独特的手感及体验。");
       
            group5.Items.Add(new SampleDataItem("Group-5-Item-1",
                    "Vertu",
                    "Item Subtitle: 1",
                    "Assets/51.jpg",
                    "VERTU是诺基亚所成立的全球第一家奢侈手机公司，以经营高档品牌的方式，由世界著名的手机设计师Frank Nuovo设计，2012年诺基亚将VERTU品牌90%的股权出售给了瑞典投资公司EQT。 该品牌的机型从外观、用料到功能都绝对称得上有王者风范，平均每款售价高达十几万元人民币。VERTU，这个起源于拉丁文的单词原意即为“高品质、独一无二”。",
                    ITEM_CONTENT51,
                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-2",
                    "mobiado stealth",
                    "Item Subtitle: 2",
                    "Assets/52.jpg",
                    "",
                    ITEM_CONTENT,
                    group5));
            group5.Items.Add(new SampleDataItem("Group-5-Item-3",
                    "诺基亚8800Arte",
                    "Item Subtitle: 3",
                    "Assets/53.jpg",
                    "",
                    ITEM_CONTENT,
                    group5));
            
            this.AllGroups.Add(group5);

            var group6 = new SampleDataGroup("Group-6",
                    "珠宝",
                    "Group Subtitle: 6",
                    "Assets/60.jpg",
                    "Group Description: Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt, lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante");
            String ITEM_CONTENT61 = String.Format("{0}\n\n",
                 "JOLEE的商品75%为进口，25%在国内生产，其饰品都为925高纯度银镶嵌顶级天然水晶制成。金属配件大多采用925高度纯银镀18k钯金和铑金（与铂金同类），保持不褪色的闪耀光泽；水晶采用从巴西、南非、南美进口的顶级天然水晶，确保其完美无瑕。JOLEE的饰品高贵、典雅，像精灵般体贴温柔，善待所有爱护它们的有缘人。");
       
            group6.Items.Add(new SampleDataItem("Group-6-Item-1",
                    "羽兰",
                    "Item Subtitle: 1",
                    "Assets/61.jpg",
                    "JOLEE（中文名：羽兰）由雅克凯恩(Yake Kaien)于1983年一手创立，主要分布法国，英国，爱尔兰，德国等九个欧洲国家。总部设立于法国巴黎，品牌隶属于法国羽兰国际集团。业务主要针对宝石与半宝石类时尚首饰。2006年首次进入亚洲市场，在香港设立亚太区总部。继而引军中国内陆市场，在中国苏州建立大中华区市场总部。",
                    ITEM_CONTENT61,
                    group6));
            String ITEM_CONTENT62 = String.Format("{0}\n\n",
                "回顾卡地亚的历史，就是回顾现代珠宝百年历史的变迁，在卡地亚的发展历程中，一直与各国的皇室贵族和社会名流保持着息息相关的联系和紧密的交往，并已成为全球时尚人士的奢华梦想。百年以来，美誉为“皇帝的珠宝商，珠宝商的皇帝”的卡地亚仍然以其非凡的创意和完美的工艺为人类创制出许多精美绝伦，无可比拟的旷世杰作。");
       
            group6.Items.Add(new SampleDataItem("Group-6-Item-2",
                    "卡地亚",
                    "Item Subtitle: 2",
                    "Assets/62.jpg",
                    "卡地亚（Cartier SA）是一间法国钟表及珠宝制造商，于1847年由Louis-François Cartier在巴黎Rue Montorgueil 31号创办。1874年，其子亚法·卡地亚继承其管理权，由其孙子路易·卡地亚、皮尔·卡地亚与积斯·卡地亚将其发展成世界著名品牌。现为瑞士历峰集团（Compagnie Financière Richemont SA）下属公司。1904年曾为飞机师阿尔拔图·山度士·度门设计世界上首只戴在手腕的腕表——卡地亚山度士腕表 （Cartier Santos）。",
                    ITEM_CONTENT62,
                    group6));
            String ITEM_CONTENT63 = String.Format("{0}\n\n",
                "Tiffany & Co.（蒂芙尼），珠宝界的皇后，以钻石和银制品着称于世。Tiffany&Co.创建于1837年，是以银制餐具出名，在1851年推出了银制925装饰品而更加着名。1960年好莱坞著名女星奥黛丽赫本出演的《蒂芙尼早餐》就是以Tiffany命名的。Tiffany，美国设计的象征。以爱与美、罗曼蒂克与梦想为主题而风誉了近两个世纪。它以充满官能的美以及柔软纤细的感性满足了世界上所有女性的幻想和欲望。");
       
            group6.Items.Add(new SampleDataItem("Group-6-Item-3",
                    "蒂芙尼",
                    "Item Subtitle: 3",
                    "Assets/63.jpg",
                    "蒂芙尼公司（英语：Tiffany & Co.，NYSE：TIF）是一间于1837年开设的美国珠宝和银饰公司。1853年查尔斯·蒂芙尼掌握了公司的控制权，将公司名称简化为“蒂芙尼公司”（Tiffany & Co），公司也从此确立了以珠宝业为经营重点。蒂芙尼逐渐在全球各大城市建立分店。蒂芙尼制定了一套自己的宝石、铂金标准，并被美国政府采纳为官方标准。时至今日，蒂芙尼是全球中知名的奢侈品公司之一。其蒂芙尼蓝色礼盒（Tiffany Blue Box）更成为美国洗练时尚独特风格的标志。",
                    ITEM_CONTENT63,
                    group6));
            
            this.AllGroups.Add(group6);
        }
    }
}

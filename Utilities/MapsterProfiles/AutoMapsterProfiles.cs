using Black_order.DTOs.CategoryDTOs;
using Black_order.DTOs.FavoriteDTOs;
using Black_order.DTOs.GuideDTOs;
using Black_order.DTOs.OrderDTOs;
using Black_order.DTOs.ProductDTOs;
using Black_order.DTOs.ReviewDTOs;
using Black_order.Models;
using Black_order.Models.Guides;
using Black_order.Models.Orders;
using Black_order.Models.Reviews;
using Mapster;
using Models.Favorite;

namespace Black_order.Utilities.MapsterProfiles
{
    public class AutoMapsterProfiles : IRegister
    {


        public void MapsterProfiles(TypeAdapterConfig config)
        {


            Register(config);
        }



        public void Register(TypeAdapterConfig config)
        {

            //*******************************PRODUCTS*******************************/

            //ORIGEN -> DESTINO
            config.NewConfig<Product, ProductDTO>()
            //DESTINO -> ORIGEN
           .Map(productDTO => productDTO.Images, product => product.Images.Select(x => x.Url).ToList())
           .Map(productDTO => productDTO.CategoryName, product => product.Category != null ? product.Category.Name : null);


            config.NewConfig<ProductCreateDTO, Product>().Ignore(product => product.Images);

            config.NewConfig<Product, AddProductsDTO>();
            config.NewConfig<AddProductsDTO, Product>();


            //config.NewConfig<ProductHistory, ProductHistoryDTO>().Map();

            /*******************************CATEGORIES*******************************/
            config.NewConfig<Category, CategoryDTO>()
                .Map(categoryDTO => categoryDTO.Images, category => category.Images.Select(x => x.Url).ToList());

            config.NewConfig<CreateCategoryDTO, Category>()
                .Ignore(x => x.Id)
                .Ignore(x => x.Images)
                .Ignore(category => category.Products);


            /*******************************GUIDES*******************************/
            config.NewConfig<Guide, GuideSummaryDTO>()
                .Map(guideDTO => guideDTO.CategoryName, guide => guide.Category != null ? guide.Category.Name : null)
                .Map(guideDTO => guideDTO.CategorySlug, guide => guide.Category != null ? guide.Category.Slug : null);

            config.NewConfig<Guide, GuideDTO>()
                .Map(guideDTO => guideDTO.CategoryName, guide => guide.Category != null ? guide.Category.Name : null)
                .Map(guideDTO => guideDTO.CategorySlug, guide => guide.Category != null ? guide.Category.Slug : null)
                .Map(guideDTO => guideDTO.Media, guide => guide.Media.OrderBy(media => media.SortOrder).ToList());

            config.NewConfig<GuideMedia, GuideMediaDTO>()
                .Map(mediaDTO => mediaDTO.MediaType, media => media.MediaType.ToString());

            config.NewConfig<GuideCategory, GuideCategoryDTO>();

            config.NewConfig<CreateGuideDTO, Guide>()
                .Ignore(guide => guide.Id)
                .Ignore(guide => guide.Slug)
                .Ignore(guide => guide.AuthorId)
                .Ignore(guide => guide.Author)
                .Ignore(guide => guide.Category)
                .Ignore(guide => guide.Media)
                .Ignore(guide => guide.CoverUrl)
                .Ignore(guide => guide.CreatedAt)
                .Ignore(guide => guide.UpdatedAt)
                .Ignore(guide => guide.PublishedAt)
                .Ignore(guide => guide.ViewCount)
                .Map(guide => guide.CategoryId, dto => string.IsNullOrWhiteSpace(dto.CategoryId) ? null : dto.CategoryId)
                .Map(guide => guide.Level, dto => string.IsNullOrWhiteSpace(dto.Level) ? null : dto.Level)
                .Map(guide => guide.Expansion, dto => string.IsNullOrWhiteSpace(dto.Expansion) ? null : dto.Expansion);


            /*******************************ORDERS*******************************/
            config.NewConfig<Order, OrderSummaryDTO>()
                .Map(orderDTO => orderDTO.Status, order => order.Status.ToString())
                .Map(orderDTO => orderDTO.PaymentStatus, order => order.PaymentStatus.ToString())
                .Map(orderDTO => orderDTO.ItemCount, order => order.Items.Count);

            config.NewConfig<Order, OrderDTO>()
                .Map(orderDTO => orderDTO.Status, order => order.Status.ToString())
                .Map(orderDTO => orderDTO.PaymentStatus, order => order.PaymentStatus.ToString())
                .Map(orderDTO => orderDTO.ItemCount, order => order.Items.Count);

            config.NewConfig<OrderItem, OrderItemDTO>()
                .Map(itemDTO => itemDTO.ImageUrl, item =>
                    item.Product != null && item.Product.Images.Count > 0
                        ? item.Product.Images.First().Url
                        : null);

            /*******************************REVIEWS*******************************/
            config.NewConfig<Review, ReviewDTO>()
                .Map(reviewDTO => reviewDTO.UserName, review =>
                    review.User != null ? review.User.UserName ?? string.Empty : string.Empty);

            /*******************************FAVORITES*******************************/
            config.NewConfig<FavoriteItems, FavoriteItemDTO>()
                .Map(itemDTO => itemDTO.ProductName, item => item.Product != null ? item.Product.Name : null)
                .Map(itemDTO => itemDTO.Price, item => item.Product != null ? item.Product.Price : 0)
                .Map(itemDTO => itemDTO.Images, item => item.Product != null
                    ? item.Product.Images.Select(image => image.Url).ToList()
                    : new List<string>());


        }



    }

}

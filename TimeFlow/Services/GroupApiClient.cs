using System.Collections.Generic;
using System.Threading.Tasks;
using TimeFlow.Models;

namespace TimeFlow.Services
{
    public class GroupApiClient
    {
        // Mock method để code biên dịch được
        public async Task<List<Group>> GetGroupsByUserIdAsync(int userId)
        {
            // Giả lập delay
            await Task.Delay(100);

            // Trả về dữ liệu mẫu
            return new List<Group>
            {
                new Group { GroupId = 1, GroupName = "Development Team" },
                new Group { GroupId = 2, GroupName = "Marketing Team" },
                new Group { GroupId = 3, GroupName = "Design Team" }
            };
        }
    }
}
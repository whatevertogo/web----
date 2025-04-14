import { api } from './api';
// 指向新的 exam 类型文件
import type { ExamStatisticsDto, ExamResultDto } from '../types/exam';

// 响应接口定义
interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

interface ExamResponse {
  id: number;
  title: string;
  description?: string;
  questions: any[];
  totalScore?: number;
  deadline?: string;
  isSubmitted?: boolean;
}

interface ExamSubmissionResponse {
  data: {
    score: number;
    totalScore: number;
    correctCount: number;
    questionCount: number;
    completionTime: number;
    submittedAt: string;
    answers: Array<{
      questionId: number;
      answer: string;
      isCorrect: boolean;
      score: number;
    }>;
  };
}

// 试卷服务
export const examService = {
  // 获取所有试卷（管理员）
  async getExams(): Promise<ApiResponse<ExamResponse[]>> {
    try {
      const response = await api.get<ApiResponse<ExamResponse[]>>('/exams');
      console.log('获取试卷原始响应:', response);

      // 如果返回空数组，创建一个空数组作为默认值
      if (!response) {
        console.warn('试卷响应为空');
        return { success: true, data: [] };
      }

      // 处理不同的响应格式
      if (response.success && Array.isArray(response.data)) {
        // 标准格式，直接返回
        return response;
      } else if (Array.isArray(response)) {
        // 如果响应本身就是数组，将其包装为标准格式
        return { success: true, data: response };
      } else if (response.data === null || response.data === undefined) {
        // 如果 data 字段为空，返回空数组
        console.warn('试卷响应的 data 字段为空');
        return { success: true, data: [] };
      }

      // 默认情况，直接返回原始响应
      return response as ApiResponse<ExamResponse[]>;
    } catch (error) {
      console.error('获取试卷列表失败:', error);
      // 返回空数组作为默认值，避免前端报错
      return { success: false, data: [] };
    }
  },

  // 获取学生的试卷
  async getStudentExams(): Promise<ExamResponse[]> {
    try {
      const response = await api.get<ExamResponse[]>('/exams');
      return Array.isArray(response) ? response : [];
    } catch (error) {
      console.error('获取学生试卷列表失败:', error);
      throw error;
    }
  },

  // 获取试卷详情
  async getExamById(id: number): Promise<ExamResponse> {
    try {
      return await api.get<ExamResponse>(`/exams/${id}`);
    } catch (error) {
      console.error(`获取试卷 ${id} 详情失败:`, error);
      throw error;
    }
  },

  // 创建试卷
  async createExam(examData: any) {
    try {
      const response = await api.post('/exams', examData);
      console.log('创建试卷响应:', response);
      return response;
    } catch (error) {
      console.error('创建试卷失败:', error);
      throw error;
    }
  },

  // 更新试卷
  async updateExam(examData: any) {
    try {
      return await api.put(`/exams/${examData.id}`, examData);
    } catch (error) {
      console.error(`更新试卷 ${examData.id} 失败:`, error);
      throw error;
    }
  },

  // 删除试卷
  async deleteExam(id: number) {
    try {
      return await api.delete(`/exams/${id}`);
    } catch (error) {
      console.error(`删除试卷 ${id} 失败:`, error);
      throw error;
    }
  },

  // 获取学生列表
  async getStudents(): Promise<ApiResponse<any[]>> {
    try {
      console.log('获取学生列表...');
      const response = await api.get<ApiResponse<any[]>>('/users/students');
      console.log('获取学生列表响应:', response);

      // 处理不同的响应格式
      if (response && response.success && Array.isArray(response.data)) {
        // 标准格式，直接返回
        return response;
      } else if (Array.isArray(response)) {
        // 如果响应本身就是数组，将其包装为标准格式
        return { success: true, data: response };
      } else if (!response || !response.data) {
        // 如果没有数据，返回空数组
        console.warn('学生列表为空');
        return { success: true, data: [] };
      }

      // 默认情况，直接返回原始响应
      return response as ApiResponse<any[]>;
    } catch (error) {
      console.error('获取学生列表失败:', error);
      throw error; // 抛出错误，让调用者处理
    }
  },

  // 分配试卷给学生
  async assignExam(examId: number, studentIds: number[]) {
    try {
      console.log(`准备分配试卷 ${examId} 给学生:`, studentIds);
      const response = await api.post(`/exams/${examId}/assign`, { studentIds });
      console.log(`分配试卷 ${examId} 响应:`, response);
      return response;
    } catch (error) {
      console.error(`分配试卷 ${examId} 失败:`, error);
      throw error; // 抛出错误，让调用者处理
    }
  },

  // 提交试卷
  async submitExam(examId: number, submission: any): Promise<ExamSubmissionResponse> {
    try {
      console.log(`正在提交试卷 ${examId}，数据:`, JSON.stringify(submission).substring(0, 200) + '...');
      
      // 添加超时处理
      const timeoutPromise = new Promise<never>((_, reject) => {
        setTimeout(() => reject(new Error('提交请求超时，请重试')), 20000); // 延长到20秒
      });

      // 实际请求
      const fetchPromise = api.post<ExamSubmissionResponse>(`/exams/${examId}/submit`, submission);

      // 使用Promise.race竞争，哪个先完成就返回哪个
      try {
        const response = await Promise.race([fetchPromise, timeoutPromise]);
        console.log(`试卷 ${examId} 提交成功，响应:`, response);
        return response;
      } catch (raceError: any) {
        console.error(`提交试卷 Promise.race 出错:`, raceError);
        throw raceError;
      }
    } catch (error: any) {
      console.error(`提交试卷 ${examId} 失败:`, error);
      
      // 针对特定错误提供更友好的消息
      if (error.message && error.message.includes('无法连接到服务器')) {
        throw new Error(`服务器连接失败，请确认后端服务已启动并且API地址正确 (${error.message})`);
      }
      
      throw error;
    }
  },

  /**
   * 获取试卷统计信息 (管理员)
   * @param examId 试卷ID
   * @returns 试卷统计数据
   */
  async getExamStatistics(examId: number): Promise<ExamStatisticsDto> {
    try {
      console.log(`准备获取试卷 ${examId} 的统计信息`);
      // 后端返回的数据结构是 { success: true, data: statistics }
      const response = await api.get<{ success: boolean; data: ExamStatisticsDto; message?: string }>(`/exams/${examId}/statistics`);
      
      if (response && response.success && response.data) {
         console.log(`获取试卷 ${examId} 统计信息响应:`, response.data);
         return response.data; // 直接返回 data 部分
      } else {
        // 如果 success 为 false 或 data 不存在，则抛出错误
        throw new Error(response?.message || `获取统计数据失败 (${examId})`);
      }
    } catch (error) {
      console.error(`获取试卷 ${examId} 统计信息失败:`, error);
      // 可以考虑向上层抛出更具体的错误，或者保留原始错误
      throw error; 
    }
  },

  // 获取试卷结果
  async getExamResults(examId: number, studentId?: number) {
    try {
      const url = studentId
        ? `/exams/${examId}/results?studentId=${studentId}`
        : `/exams/${examId}/results`;
      return await api.get(url);
    } catch (error) {
      console.error(`获取试卷 ${examId} 结果失败:`, error);
      throw error;
    }
  },

  // 导出试卷结果
  async exportExamResults(examId: number) {
    try {
      return await api.get(`/exams/${examId}/export`, {
        responseType: 'blob'
      });
    } catch (error) {
      console.error(`导出试卷 ${examId} 结果失败:`, error);
      throw error;
    }
  }
};

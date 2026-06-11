import type { AxiosError } from 'axios';

export interface FieldError {
  field: string;
  messages: string[];
}

export interface ApiError {
  type: 'validation-error' | 'not-found' | 'business-error' | 'unauthorized' | 'forbidden' | 'server-error' | 'network-error';
  title: string;
  message: string;
  statusCode?: number;
  fieldErrors?: FieldError[];
  code?: string;
}

export const parseApiError = (error: unknown): ApiError => {
  // Network error (no response)
  if (error instanceof Error && 'isAxiosError' in error) {
    const axiosError = error as AxiosError;
    
    if (!axiosError.response) {
      return {
        type: 'network-error',
        title: 'Network Error',
        message: axiosError.message || 'Unable to connect to the server. Please check your internet connection.',
      };
    }

    const response = axiosError.response;
    const data = response.data as any;
    const statusCode = response.status;

    // Parse ProblemDetails response from backend
    if (data && typeof data === 'object') {
      const type = data.type as string;
      const title = data.title as string;
      const errors = data.errors as Record<string, string[]>;
      const code = data.code as string;

      // Validation errors (400)
      if (statusCode === 400 && type === 'validation-error') {
        const fieldErrors: FieldError[] = [];
        if (errors) {
          Object.entries(errors).forEach(([field, messages]) => {
            fieldErrors.push({ field, messages });
          });
        }
        return {
          type: 'validation-error',
          title: title || 'Validation Failed',
          message: 'Please correct the errors and try again.',
          statusCode,
          fieldErrors,
        };
      }

      // Not found errors (404)
      if (statusCode === 404 && type === 'not-found') {
        return {
          type: 'not-found',
          title: 'Not Found',
          message: title || 'The requested resource was not found.',
          statusCode,
        };
      }

      // Business errors (422)
      if (statusCode === 422 && type === 'business-error') {
        return {
          type: 'business-error',
          title: 'Business Rule Violation',
          message: title || 'A business rule was violated.',
          statusCode,
          code,
        };
      }

      // Unauthorized (401)
      if (statusCode === 401 && type === 'unauthorized') {
        return {
          type: 'unauthorized',
          title: 'Unauthorized',
          message: title || 'You are not authorized to perform this action.',
          statusCode,
        };
      }

      // Forbidden (403)
      if (statusCode === 403 && type === 'forbidden') {
        return {
          type: 'forbidden',
          title: 'Forbidden',
          message: title || 'You do not have permission to access this resource.',
          statusCode,
        };
      }
    }

    // Generic error based on status code
    if (statusCode === 400) {
      return {
        type: 'validation-error',
        title: 'Bad Request',
        message: data?.message || 'The request was invalid.',
        statusCode,
      };
    }

    if (statusCode === 404) {
      return {
        type: 'not-found',
        title: 'Not Found',
        message: data?.message || 'The requested resource was not found.',
        statusCode,
      };
    }

    if (statusCode === 422) {
      return {
        type: 'business-error',
        title: 'Business Rule Violation',
        message: data?.message || 'A business rule was violated.',
        statusCode,
      };
    }

    if (statusCode === 401) {
      return {
        type: 'unauthorized',
        title: 'Unauthorized',
        message: data?.message || 'You are not authorized to perform this action.',
        statusCode,
      };
    }

    if (statusCode === 403) {
      return {
        type: 'forbidden',
        title: 'Forbidden',
        message: data?.message || 'You do not have permission to access this resource.',
        statusCode,
      };
    }

    // Server error (500+)
    return {
      type: 'server-error',
      title: 'Server Error',
      message: data?.message || 'An unexpected error occurred on the server.',
      statusCode,
    };
  }

  // Generic error fallback
  return {
    type: 'server-error',
    title: 'Error',
    message: error instanceof Error ? error.message : 'An unknown error occurred.',
  };
};

export const getFieldErrorMessage = (apiError: ApiError, fieldName: string): string | null => {
  if (apiError.type !== 'validation-error' || !apiError.fieldErrors) {
    return null;
  }

  const fieldError = apiError.fieldErrors.find(fe => fe.field.toLowerCase() === fieldName.toLowerCase());
  return fieldError ? fieldError.messages[0] : null;
};

export const getToastMessage = (apiError: ApiError): string => {
  switch (apiError.type) {
    case 'validation-error':
      return apiError.message;
    case 'not-found':
      return apiError.message;
    case 'business-error':
      return apiError.message;
    case 'unauthorized':
      return apiError.message;
    case 'forbidden':
      return apiError.message;
    case 'server-error':
      return apiError.message;
    case 'network-error':
      return apiError.message;
    default:
      return 'An unexpected error occurred.';
  }
};

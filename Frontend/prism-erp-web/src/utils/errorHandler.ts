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

const FORBIDDEN_MESSAGE = 'You do not have permission to perform this action.';

export const parseApiError = (error: unknown): ApiError => {
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
    const data = response.data as Record<string, unknown> | undefined;
    const statusCode = response.status;

    if (data && typeof data === 'object') {
      const type = (data.type ?? data.Type) as string | undefined;
      const title = (data.title ?? data.Title) as string | undefined;
      const detail = (data.detail ?? data.Detail) as string | undefined;
      const errors = (data.errors ?? data.Errors) as Record<string, string[]> | undefined;
      const code = (data.code ?? data.Code) as string | undefined;
      const problemMessage = detail || title;

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

      if (statusCode === 404 && type === 'not-found') {
        return {
          type: 'not-found',
          title: 'Not Found',
          message: problemMessage || 'The requested resource was not found.',
          statusCode,
        };
      }

      if (statusCode === 422 && type === 'business-error') {
        return {
          type: 'business-error',
          title: 'Business Rule Violation',
          message: problemMessage || 'A business rule was violated.',
          statusCode,
          code,
        };
      }

      if (statusCode === 401 && type === 'unauthorized') {
        return {
          type: 'unauthorized',
          title: 'Unauthorized',
          message: problemMessage || 'You are not authorized to perform this action.',
          statusCode,
        };
      }

      if (statusCode === 403 && type === 'forbidden') {
        return {
          type: 'forbidden',
          title: 'Access Denied',
          message: problemMessage || FORBIDDEN_MESSAGE,
          statusCode,
        };
      }
    }

    if (statusCode === 400) {
      return {
        type: 'validation-error',
        title: 'Bad Request',
        message: (data?.message as string) || 'The request was invalid.',
        statusCode,
      };
    }

    if (statusCode === 404) {
      return {
        type: 'not-found',
        title: 'Not Found',
        message: (data?.message as string) || 'The requested resource was not found.',
        statusCode,
      };
    }

    if (statusCode === 422) {
      return {
        type: 'business-error',
        title: 'Business Rule Violation',
        message: (data?.message as string) || 'A business rule was violated.',
        statusCode,
      };
    }

    if (statusCode === 401) {
      return {
        type: 'unauthorized',
        title: 'Unauthorized',
        message: (data?.detail as string) || (data?.message as string) || 'You are not authorized to perform this action.',
        statusCode,
      };
    }

    if (statusCode === 403) {
      return {
        type: 'forbidden',
        title: 'Access Denied',
        message: (data?.detail as string) || (data?.message as string) || FORBIDDEN_MESSAGE,
        statusCode,
      };
    }

    return {
      type: 'server-error',
      title: 'Server Error',
      message: (data?.message as string) || 'An unexpected error occurred on the server.',
      statusCode,
    };
  }

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

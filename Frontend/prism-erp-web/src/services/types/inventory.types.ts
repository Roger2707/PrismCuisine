// Inventory Module Types

export interface ProductDto {
  id: number;
  categoryId: number;
  sku: string;
  name: string;
  unit: string;
  description?: string;
  isActive: boolean;
}

export interface CreateProductRequest {
  categoryId: number;
  sku: string;
  name: string;
  unit: string;
  description?: string;
}

export interface UpdateProductRequest {
  categoryId: number;
  name: string;
  unit: string;
  description?: string;
}

export interface ProductCategoryDto {
  id: number;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface CreateProductCategoryRequest {
  code: string;
  name: string;
  description?: string;
}

export interface UpdateProductCategoryRequest {
  name: string;
  description?: string;
}

export interface WarehouseDto {
  id: number;
  code: string;
  name: string;
  location?: string;
  isActive: boolean;
}

export interface CreateWarehouseRequest {
  code: string;
  name: string;
  location?: string;
}

export interface UpdateWarehouseRequest {
  name: string;
  location?: string;
}

export interface InventoryBalanceDto {
  id: number;
  productId: number;
  warehouseId: number;
  quantityOnHand: number;
  reservedQuantity: number;
  availableQuantity: number;
  reorderLevel: number;
  isBelowReorderLevel: boolean;
}

export interface InventoryMovementDto {
  id: number;
  inventoryBalanceId: number;
  movementType: string;
  quantity: number;
  unitCost: number;
  referenceType: string;
  reference?: string;
  referenceId?: number;
  notes?: string;
  createdAt: string;
}

export interface InventoryCostLayerDto {
  id: number;
  inventoryBalanceId: number;
  quantityReceived: number;
  quantityRemaining: number;
  unitCost: number;
  receivedAt: string;
}

export interface InventoryReservationDto {
  id: number;
  inventoryBalanceId: number;
  quantity: number;
  fulfilledQuantity: number;
  remainingQuantity: number;
  status: string;
  referenceType: string;
  referenceId: number;
  notes?: string;
}

export interface CreateInventoryBalanceRequest {
  productId: number;
  warehouseId: number;
  reorderLevel: number;
}

export interface ReceiveInventoryRequest {
  productId: number;
  warehouseId: number;
  quantity: number;
  unitCost: number;
  reference?: string;
  referenceId?: number;
  notes?: string;
}

export interface IssueInventoryRequest {
  productId: number;
  warehouseId: number;
  quantity: number;
  reference?: string;
  referenceId?: number;
  notes?: string;
}

export interface AdjustInventoryRequest {
  productId: number;
  warehouseId: number;
  newQuantity: number;
  unitCostForIncrease: number;
  reference?: string;
  notes?: string;
}

export interface CreateReservationLine {
  productId: number;
  warehouseId: number;
  quantity: number;
  referenceId: number;
  notes?: string;
}

export interface CreateReservationRequest {
  createReservationLines: CreateReservationLine[];
}
